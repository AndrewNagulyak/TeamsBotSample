using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using System.Linq;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema.Teams;
using ProactiveBot.Models.Messages;
using ProactiveBot.AdaptiveCardRepository;

namespace ProactiveBot.Controllers
{
    [Route("api/notify")]
    [ApiController]
    public class NotifyController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly string _appId;
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        private readonly string[] _cards =
      {
            Path.Combine(".", "Resources", "ApproveCard.json"),
        };
        public NotifyController(IBotFrameworkHttpAdapter adapter, IConfiguration configuration, ConcurrentDictionary<string, ConversationReference> conversationReferences)
        {
            _adapter = adapter;
            _conversationReferences = conversationReferences;
            _appId = configuration["MicrosoftAppId"];

            // If the channel is the Emulator, and authentication is not in use,
            // the AppId will be null.  We generate a random AppId for this case only.
            // This is not required for production, since the AppId will have a value.
            if (string.IsNullOrEmpty(_appId))
            {
                _appId = Guid.NewGuid().ToString(); //if no AppId, use a random Guid
            }
        }
        [HttpPost]
        public async Task<IActionResult> Data([FromBody] MessageList notifyMessages)
        {
            var i = 0;

            if (notifyMessages.MessageType == "2")
            {
                foreach (var conversationReference in _conversationReferences.Values)
                {
                    
                    await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, async (ITurnContext turnContext, CancellationToken cancellationToken) =>
                    {
                        IEnumerable<TeamsChannelAccount> members = TeamsInfo.GetMembersAsync(turnContext, cancellationToken).Result;
                        foreach (var member in members)
                        {
                            foreach (var notifyMessage in notifyMessages.messages)
                            {
                                if (notifyMessage.AssignedTo == member.UserPrincipalName)
                                {
                                    i++;
                                    SecondMessageType message = new SecondMessageType();
                                    message.AssignedTo = notifyMessage.AssignedTo;
                                    message.MessageType = notifyMessage.MessageType;
                                    message.InProgressTasks = notifyMessage.InProgressTasks;
                                    message.Link = notifyMessage.Link;
                                    message.TerminateTasks = notifyMessage.TerminateTasks;
                                    message.NewTasks = notifyMessage.NewTasks;
                                    var cardAttachment = AdaptiveCardFactory.CreateAdaptiveCardSecondTypeAttachment(member.Name, message);
                                    await turnContext.SendActivityAsync(MessageFactory.Attachment(cardAttachment), cancellationToken);
                                }
                            }
                        }
                    }, default(CancellationToken));
                }
            }
            return new ContentResult()
            {
                Content = "<html><body><h1>Proactive messages have been sent:" + i + "users" + "</h1></body></html>",
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
            };
        }
        [HttpPost("{userid?}")]
        public async Task<IActionResult> Post(string userid, [FromBody] NotifyMessage notifyMessage)
        {
            var sb = new StringBuilder();

            bool flag = false;
            if (!string.IsNullOrEmpty(userid))
            {
                foreach (var conversationReference in _conversationReferences.Values)
                {

                    await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, async (ITurnContext turnContext, CancellationToken cancellationToken) =>
                    {
                        IEnumerable<TeamsChannelAccount> members = TeamsInfo.GetMembersAsync(turnContext, cancellationToken).Result;
                        //var connector = new ConnectorClient(new Uri(turnContext.Activity.ServiceUrl));
                        // members =  connector.Conversations.GetConversationMembersAsync(conversationReference.Conversation.Id).Result;

                        //// Concatenate information about all the members into a string
                        foreach (var member in members)
                        {
                            sb.AppendLine($"GivenName = {member.Name}, Email = {member.Email}, User Principal Name {member.UserPrincipalName}, TeamsMemberId = {member.Id}, members = {members.ToList().Count},{_conversationReferences.Values.Count}");
                            if (userid == member.UserPrincipalName)
                            {
                                switch (notifyMessage.MessageType)
                                {
                                    case "1":
                                    case "3":
                                        {
                                            flag = true;
                                           
                                            ThirdMessageType message = new ThirdMessageType();
                                            message.AssignedTo = notifyMessage.AssignedTo;
                                            message.MessageType = notifyMessage.MessageType;
                                            message.IDCard = notifyMessage.IDCard;
                                            message.IDTask = notifyMessage.IDTask;
                                            message.LibDispName = notifyMessage.LibDispName;
                                            message.TitleTask = notifyMessage.TitleTask;
                                            message.Link = notifyMessage.Link;
                                            message.TaskType = notifyMessage.TaskType;
                                            if (message.TaskType == "LSTaskAppruve" || message.TaskType == "LSTaskExecute")
                                            {
                                                var cardAttachment = AdaptiveCardFactory.CreateAdaptiveCardForSubmitAttachment(message);
                                                var req = await turnContext.SendActivityAsync(MessageFactory.Attachment(cardAttachment), cancellationToken);
                                                message.Key = req.Id;
                                                var cardWithId = AdaptiveCardFactory.CreateAdaptiveCardForSubmitAttachment(message);
                                                var requestWithId = MessageFactory.Attachment(cardWithId);
                                                requestWithId.Id = req.Id;
                                                await turnContext.UpdateActivityAsync(requestWithId, cancellationToken);
                                            }
                                            else
                                            {
                                                var cardAttachment=AdaptiveCardFactory.CreateAdaptiveCardThirdTypeAttachment(message); ;
                                                if (message.MessageType == "3")
                                                {
                                                     cardAttachment = AdaptiveCardFactory.CreateAdaptiveCardThirdTypeAttachment(message);
                                                }
                                                else
                                                {
                                                     cardAttachment = AdaptiveCardFactory.CreateAdaptiveCardFirstTypeAttachment(message);

                                                }
                                                await turnContext.SendActivityAsync(MessageFactory.Attachment(cardAttachment), cancellationToken);
                                            }
                                            break;
                                        }
                                    case "2":
                                        {
                                            flag = true;

                                            SecondMessageType message = new SecondMessageType();
                                            message.AssignedTo = notifyMessage.AssignedTo;
                                            message.MessageType = notifyMessage.MessageType;
                                            message.InProgressTasks = notifyMessage.InProgressTasks;
                                            message.Link = notifyMessage.Link;
                                            message.TerminateTasks = notifyMessage.TerminateTasks;
                                            message.NewTasks = notifyMessage.NewTasks;
                                            var cardAttachment = AdaptiveCardFactory.CreateAdaptiveCardSecondTypeAttachment(member.Name, message);
                                            await turnContext.SendActivityAsync(MessageFactory.Attachment(cardAttachment), cancellationToken);
                                            break;
                                        }
                                    


                                }
                            }
                        }
                    }, default(CancellationToken));
                }
            }
            else
            {
                foreach (var conversationReference in _conversationReferences.Values)
                {
                    await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, (ITurnContext turnContext, CancellationToken cancellationToken) => turnContext.SendActivityAsync("withoutUser"), default(CancellationToken));
                }
            }


            // Let the caller know proactive messages have been sent
            return new ContentResult()
            {
                Content = "<html><body><h1>Proactive messages have been sent:" + userid + "status = " + flag + "data =  " + sb.ToString() + "</h1></body></html>",
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
            };
        }


    }










}
