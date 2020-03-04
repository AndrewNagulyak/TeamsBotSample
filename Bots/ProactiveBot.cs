// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProactiveBot.AdaptiveCardRepository;
using ProactiveBot.Models;
using ProactiveBot.Models.Messages;

namespace Microsoft.BotBuilderSamples
{
    public class ProactiveBot : TeamsActivityHandler
    {
        private static readonly HttpClient client = new HttpClient();

        // Message to send to users when the bot receives a Conversation Update event
        private const string WelcomeMessage = @"```
Привіт! 🙂 
Я бот для роботи з СЕД LSDocs. Мої основні функції:
    - отримання 10 останніх завдань, які можна затвердити (просто напишіть мені 'TopTen'); 
    - можливість виконання завдань безпосередньо у боті(Lazy approvals);
    - сповіщення про призначення нової задачі;
    - щоденні сповіщення про поточний статус особистих задач;
    - сповіщення за 24 години про протермінування задач.
А також я постійно навчаюсь, тому можливостей буде більше.🙂 
По всім питанням звертайтесь до support@lizard-soft.com```";

        // Dependency injected dictionary for storing ConversationReference objects used in NotifyController to proactively message users
        private ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        private BotState _conversationState;
        private BotState _userState;

        public ProactiveBot(ConcurrentDictionary<string, ConversationReference> conversationReferences, ConversationState conversationState, UserState userState)
        {
            _conversationReferences = conversationReferences;
            _conversationState = conversationState;
            _userState = userState;
        }

        private void AddConversationReference(Activity activity)
        {
            var conversationReference = activity.GetConversationReference();
            _conversationReferences.AddOrUpdate(conversationReference.User.Id, conversationReference, (key, newValue) => conversationReference);
        }

        protected override Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            AddConversationReference(turnContext.Activity as Activity);


            return base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {

            await turnContext.SendActivityAsync(MessageFactory.Text(WelcomeMessage), cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            AddConversationReference(turnContext.Activity as Activity);
            if (turnContext.Activity.Value != null)
            {

                JObject jObject = JObject.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(turnContext.Activity.Value));
                bool isCommented = false;
                var json = JsonConvert.SerializeObject(turnContext.Activity.Value);
                var desc = 0;

                var key = jObject["Key"].ToString();

                ThirdMessageType message = new ThirdMessageType();
                message.Type = jObject["Type"].ToString();


                var approve = jObject["Approved"].ToString();
                message.IDCard = jObject["IDCard"].ToString();
                message.IDTask = jObject["IDTask"].ToString();
                message.TaskType = jObject["TaskType"].ToString();
                message.Key = jObject["Key"].ToString();
                message.AssignedTo = jObject["AssignedTo"].ToString();
                message.MessageType = jObject["MessageType"].ToString();

                message.LibDispName = jObject["LibDispName"].ToString();
                message.TitleTask = jObject["TitleTask"].ToString();
                message.Link = jObject["Link"].ToString();
                message.Comment = "";

                message.TaskType = jObject["TitleTask"].ToString();
                if (approve == "Rejected")
                {
                    message.Comment = jObject["Comment"].ToString();
                }
                else
                {
                    message.Comment = jObject["ApproveComment"].ToString();
                }
                message.CardType = "submitted";
                var myUpdatedCard = AdaptiveCardFactory.CreateAdaptiveCardAfterSubmitAttachment(message, approve, message.Comment);
                if (approve == "Rejected" && string.IsNullOrEmpty(message.Comment.Trim()))
                {
                    desc = 1;
                    message.CardType = "comment";
                    isCommented = true;
                    myUpdatedCard = AdaptiveCardFactory.CreateAdaptiveCardCommentRequiredAttachment(message);
                }

                else
                {
                    var data = new StringContent(json, Encoding.UTF8, "application/json");

                    var url = "https://prod-67.westeurope.logic.azure.com:443/workflows/5ac4ad090e0e442887e67aa2319ae3ea/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=q1Uz83atZhsoR5aR3eb742pv9tqrhWmAsL5Gj2q2Lv8";
                    myUpdatedCard = AdaptiveCardFactory.CreateAdaptiveCardWaitingAttachment(message);
                    var newActivityForWait = MessageFactory.Attachment(myUpdatedCard);
                    newActivityForWait.Id = key;
                    switch (message.Type)
                    {
                        case "message":
                            {
                                UpdateMessage(turnContext, cancellationToken, newActivityForWait);
                                break;

                            }
                        case "carousel":
                            {
                                message.CardType = "wait";
                                await UpdateCarousel(turnContext, cancellationToken, myUpdatedCard, message, 1);
                                break;
                            }
                    }

                    var response = await client.PostAsync(url, data);
                    var contents = await response.Content.ReadAsStringAsync();

                    JObject jObjectResponse = JObject.Parse(contents);

                    var succesfullApprove = jObjectResponse["Approved"].ToString();
                    var status = jObjectResponse["Status"].ToString().Trim();
                    status = status == "Done" ? "Approved" : status == "Back" ? "Rejected" : "Error";
                    message.Approved = status;

                    switch (succesfullApprove)
                    {
                        case "0":
                            {
                                myUpdatedCard = AdaptiveCardFactory.CreateAdaptiveCardAfterSubmitAttachment(message, status, message.Comment);
                                break;

                            }
                        case "1":
                            {
                                myUpdatedCard = AdaptiveCardFactory.CreateAdaptiveCardAlreadySubmitAttachment(message, status);
                                break;
                            }
                    }


                }

                var newActivity = MessageFactory.Attachment(myUpdatedCard);
                newActivity.Id = key;

                // var connector = new ConnectorClient(new Uri(turnContext.Activity.ServiceUrl));
                //await connector.Conversations.DeleteActivityAsync(turnContext.Activity.Conversation.Id, key, cancellationToken);
                switch (message.Type)
                {
                    case "message":
                        {
                            UpdateMessage(turnContext, cancellationToken, newActivity);
                            break;

                        }
                    case "carousel":
                        {
                            if (!isCommented)
                            {
                                message.CardType = "submitted";
                            }
                            await UpdateCarousel(turnContext, cancellationToken, myUpdatedCard, message, 1);
                            break;
                        }
                }
                await Task.Delay(500);
                switch (message.Type)
                {
                    case "message":
                        {
                            UpdateMessage(turnContext, cancellationToken, newActivity);
                            break;

                        }
                    case "carousel":
                        {
                            if (!isCommented)
                            {
                                message.CardType = "submitted";
                            }
                            await UpdateCarousel(turnContext, cancellationToken, myUpdatedCard, message, desc);
                            break;
                        }
                }
                //string approved = jObject["approved"].ToString();

            }
            else
            {
                var userStateAccessors = _userState.CreateProperty<UserProfile>(nameof(UserProfile));
                var userProfile = await userStateAccessors.GetAsync(turnContext, () => new UserProfile());
                if (userProfile.IsSend == true)
                {

                    turnContext.Activity.RemoveRecipientMention();
                    switch (turnContext.Activity.Text.Trim())
                    {
                        case "Help":

                            await turnContext.SendActivityAsync(MessageFactory.Text($"Я бот для роботи з LSDocs. Мої основні функції: \n\n" +
                                $"- отримання 10 останніх завдань, які можна затвердити (просто напишіть мені 'TopTen'); \n\n " +
                                $"- можливість виконання завдань безпосередньо у боті (Lazy approvals); \n\n" +
                                $"- сповіщення про призначення нової задачі; \n\n" +
                                $"- щоденні сповіщення про поточний статус особистих задач; \n\n" +
                                $"- сповіщення за 24 години про протермінування задач. \n\n" +
                                $"А також я постійно навчаюсь, тому можливостей буде більше.🙂  \n\n" +
                                $" \n\n " +
                                $"Якщо у Вас є питання чи пропозиції щодо моєї роботи зв'яжіться з моєю службою підтримки почтою support@lizard-soft.com чи за телефоном +38 044 232 95 09. "), cancellationToken);
                            break;
                        case "TopTen":
                            {

                                userProfile.IsSend = false;

                                var credentials = new MicrosoftAppCredentials("4e2e9e85-b2ba-4557-9082-706d081a64e0", "f+#o^wOr%9SPfaJXrow26^]{");
                                var connector = new ConnectorClient(new Uri(turnContext.Activity.ServiceUrl), credentials);
                                var conversationId = turnContext.Activity.Conversation.Id;
                                var conversationStateAccessors = _conversationState.CreateProperty<ConversationData>(nameof(ConversationData));
                                var conversationData = await conversationStateAccessors.GetAsync(turnContext, () => new ConversationData());
                                IEnumerable<TeamsChannelAccount> members = TeamsInfo.GetMembersAsync(turnContext, cancellationToken).Result;
                                var user = new RequestBody();
                                foreach (var member in members)
                                {
                                    user = new RequestBody() { AssignedTo = member.UserPrincipalName };
                                    if (string.IsNullOrEmpty(user.AssignedTo))
                                    {
                                        throw new Exception("no user");
                                    }
                                    if (!string.IsNullOrEmpty(userProfile.CarouselId) && userProfile.Count == 1)
                                    {

                                        userProfile.Count--;
                                        await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
                                        await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
                                        await turnContext.DeleteActivityAsync(userProfile.CarouselId, cancellationToken);
                                        conversationData.PromptedUserCarousel = false;
                                        userProfile.CarouselId = "";
                                        userProfile.messagesCarousel.Clear();
                                    }

                                    List<Attachment> attachments = new List<Attachment>();
                                    IMessageActivity carousel = MessageFactory.Carousel(attachments);
                                    List<ThirdMessageType> messages = new List<ThirdMessageType>();

                                    var json = JsonConvert.SerializeObject(user);
                                    var waitReq = await turnContext.SendActivityAsync(MessageFactory.Text($"Зачекайте! Вигружаємо задачі з LSDOCS..."), cancellationToken);

                                    const string link = "https://prod-10.westeurope.logic.azure.com/workflows/87b2e250b3624ef79777ecfdb37ea0bb/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=jSY4t__HFAn16knFTcmEEqWJl2HZYH4rLHu6rDzdf8U";
                                    var data = new StringContent(json, Encoding.UTF8, "application/json");

                                    var response = await client.PostAsync(link, data);



                                    var contents = await response.Content.ReadAsStringAsync();
                                    JArray array;
                                    JObject jObjectResponse = JObject.Parse(contents);

                                    try
                                    {

                                        array = JArray.Parse(jObjectResponse["messages"].ToString());
                                    }
                                    catch (Exception ex)
                                    {
                                        throw (new Exception("Flow return bad top ten"));
                                    }
                                    if(array.Count == 0)
                                    {
                                        await turnContext.SendActivityAsync(MessageFactory.Text($"Всі задачі виконані 🙂"), cancellationToken);
                                        await turnContext.DeleteActivityAsync(waitReq.Id, cancellationToken);

                                        return;
                                    }


                                    //if (messages.Count != 0)
                                    //{
                                    //    await turnContext.DeleteActivityAsync(carousel.Id, cancellationToken);
                                    //    carousel.Attachments.Clear();
                                    //    attachments.Clear();

                                    //    messages.Clear();
                                    //}

                                    foreach (JObject cards in array.Children<JObject>())
                                    {
                                        var parameters = new Dictionary<string, string>();
                                        foreach (JProperty prop in cards.Properties())
                                        {
                                            parameters.Add(prop.Name, prop.Value.ToString());
                                        }

                                        ThirdMessageType message = new ThirdMessageType();
                                        message.AssignedTo = parameters["AssignedTo"].ToString();
                                        message.MessageType = parameters["MessageType"].ToString();
                                        message.IDCard = parameters["IDCard"].ToString();
                                        message.IDTask = parameters["IDTask"].ToString();
                                        message.LibDispName = parameters["LibDispName"].ToString();
                                        //message.TaskType = parameters["TaskType"].ToString();
                                        message.TaskType = "";

                                        message.TitleTask = parameters["TitleTask"].ToString();
                                        message.Link = parameters["Link"].ToString();
                                        message.Approved = "";

                                        message.Key = "carousel";

                                        messages.Add(message);

                                        var adaptiveCardAttachment = AdaptiveCardFactory.CreateAdaptiveCardForSubmitAttachment(message);
                                        carousel.Attachments.Add(adaptiveCardAttachment);
                                    }
                                    await turnContext.DeleteActivityAsync(waitReq.Id, cancellationToken);

                                    if (string.IsNullOrEmpty(userProfile.CarouselId) && userProfile.Count < 1)
                                    {
                                        userProfile.Count++;
                                        await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
                                        await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
                                        var req = await turnContext.SendActivityAsync(carousel, cancellationToken);
                                        var newAttachments = new List<Attachment>();
                                        newAttachments.AddRange(carousel.Attachments);
                                        carousel.Attachments.Clear();
                                        attachments.Clear();

                                        var newCarousel = MessageFactory.Carousel(attachments);
                                        foreach (var message in messages)
                                        {

                                            message.Key = req.Id;
                                            message.Type = "carousel";


                                            var adaptiveCardAttachment = AdaptiveCardFactory.CreateAdaptiveCardForSubmitAttachment(message);
                                            newCarousel.Attachments.Add(adaptiveCardAttachment);
                                        }


                                        newCarousel.Id = req.Id;

                                        userProfile.messagesCarousel = messages;

                                        userProfile.CarouselId = req.Id;
                                        conversationData.PromptedUserCarousel = true;


                                        await turnContext.UpdateActivityAsync(newCarousel, cancellationToken);
                                        userProfile.IsSend = true;

                                        await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
                                        await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
                                        return;
                                    }
                                    break;
                                }
                                break;

                            }
                        default:
                            // Echo back what the user said
                            await turnContext.SendActivityAsync(MessageFactory.Text($"Ви ввели комманду, якої я ще не знаю. Спробуйте написати 'Help'"), cancellationToken);
                            break;
                    }


                }
                else
                {
                    return;
                }
            }

        }
        private async void UpdateMessage(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken, IMessageActivity newActivity)
        {

            await turnContext.UpdateActivityAsync(newActivity, cancellationToken);
        }
        private async Task UpdateCarousel(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken, Attachment newActivity, ThirdMessageType message, int desc = 0)
        {
            var userStateAccessors = _userState.CreateProperty<UserProfile>(nameof(UserProfile));
            var userProfile = await userStateAccessors.GetAsync(turnContext, () => new UserProfile());
            List<ThirdMessageType> newMessages = new List<ThirdMessageType>();
            List<Attachment> attachments = new List<Attachment>();

            var newCarousel = MessageFactory.Carousel(attachments);
            if (desc == 1)
            {
                newMessages.Add(message);
                newCarousel.Attachments.Add(newActivity);
            }





            var messages = new List<ThirdMessageType>();
            foreach (var messageInCarousel in userProfile.messagesCarousel)
            {
                messages.Add(messageInCarousel);
            }

            foreach (var card in messages)
            {

                 



                var adaptiveCardAttachment = AdaptiveCardFactory.CreateAdaptiveCardForSubmitAttachment(card);
                switch (card.CardType)
                {
                    case "wait":
                        adaptiveCardAttachment = AdaptiveCardFactory.CreateAdaptiveCardForSubmitAttachment(card);

                        break;
                    case "submitted":
                        adaptiveCardAttachment = AdaptiveCardFactory.CreateAdaptiveCardAfterSubmitAttachment(card, card.Approved, card.Comment);

                        break;
                    case "comment":
                        adaptiveCardAttachment = AdaptiveCardFactory.CreateAdaptiveCardCommentRequiredAttachment(card);

                        break;
                }
                if (card.IDCard != message.IDCard)
                {

                    newCarousel.Attachments.Add(adaptiveCardAttachment);
                    newMessages.Add(card);

                }

            }
            if (desc == 0)
            {
                newMessages.Add(message);

                newCarousel.Attachments.Add(newActivity);
            }
            userProfile.messagesCarousel.Clear();
            foreach (var messageInCarousel in newMessages)
            {

                userProfile.messagesCarousel.Add(messageInCarousel);

            }

            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);

            newCarousel.Id = userProfile.CarouselId;
            await turnContext.UpdateActivityAsync(newCarousel, cancellationToken);


            //newCarousel.Id = message.Key;
            //await turnContext.UpdateActivityAsync(newCarousel, cancellationToken);

        }
        private static IEnumerable<JToken> AllChildren(JToken json)
        {
            foreach (var c in json.Children())
            {
                yield return c;
                foreach (var cc in AllChildren(c))
                {
                    yield return cc;
                }
            }
        }

    }
    public class RequestBody
    {
        public string AssignedTo { get; set; }

    }
}
