using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using System.Linq;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema.Teams;

namespace ProactiveBot.Controllers
{
    [Route("api/notify/{userid?}")]
    [ApiController]
    public class NotifyController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly string _appId;
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;


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

        public async Task<IActionResult> Post(string userid, [FromBody] NotifyMessage notifyMessage)
        {
            var sb = new StringBuilder();

            bool flag = false;
            if (!string.IsNullOrEmpty(userid))
            {
                foreach (var conversationReference in _conversationReferences.Values)
                {

                    await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, (ITurnContext turnContext, CancellationToken cancellationToken) =>
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
                                        {
                                            flag = true;
                                            FirstMessageType message = new FirstMessageType();
                                            message.IDCard = notifyMessage.IDCard;
                                            message.IDTask = notifyMessage.IDTask;
                                            message.LibDispName = notifyMessage.LibDispName;
                                            message.TitleTask = notifyMessage.TitleTask;
                                            message.Link = notifyMessage.IDCard;
                                            return turnContext.SendActivityAsync(notifyMessage.BuildFirstMessage(message));
                                        }
                                    case "2":
                                        {
                                            flag = true;

                                            SecondMessageType message = new SecondMessageType();
                                            message.InProgressTasks = notifyMessage.InProgressTasks;
                                            message.Link = notifyMessage.Link;
                                            message.TerminateTasks = notifyMessage.TerminateTasks;
                                            message.NewTasks = notifyMessage.NewTasks;
                                            return turnContext.SendActivityAsync(notifyMessage.BuildSecondMessage(member.Name,message));
                                        }
                                    case "3":
                                        {
                                            flag = true;

                                            ThirdMessageType message = new ThirdMessageType();
                                            message.IDCard = notifyMessage.IDCard;
                                            message.IDTask = notifyMessage.IDTask;
                                            message.LibDispName = notifyMessage.LibDispName;
                                            message.TitleTask = notifyMessage.TitleTask;
                                            message.Link = notifyMessage.IDCard;
                                            return turnContext.SendActivityAsync(notifyMessage.BuildThirdMessage(message));
                                        }


                                }
                            }
                        }
                        return null;
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
                Content = "<html><body><h1>Proactive messages have been sent:" + userid + "status = " + flag+ "data =  " + sb.ToString() +"</h1></body></html>",
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
            };
        }

    }

    public class NotifyMessage
    {
        public string BuildFirstMessage(FirstMessageType message)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Вам назаченна задача {message.TitleTask} ID = {message.IDTask},")
                .AppendLine($"[карточка №{message.IDCard}]({message.Link}), библиотека = {message.LibDispName}.");
            return sb.ToString();
        }
        public string BuildSecondMessage(string displayName, SecondMessageType message)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"{displayName}, в Вашем личном кабинете LSDOCS \n")
                .AppendLine($"{message.NewTasks} новых задач\n")
                .AppendLine($"{message.InProgressTasks} задач в роботе\n ")
                .AppendLine($"{message.NewTasks} проверочнных задач\n")
                .AppendLine($"[Чтобы перейти в личный кабинет нажмите здесь]({message.Link})\n");

            return sb.ToString();
        }
        public string BuildThirdMessage(ThirdMessageType message)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Задача {message.TitleTask} ID = {message.IDTask},")
                .AppendLine($"[в карточке №{message.IDCard}]({message.Link})в библиотеке {message.LibDispName}, будет простроченна через 24 часа.");
            return sb.ToString();
        }
        public string MessageType { get; set; }
        public string AssignedTo { get; set; }
        public string TitleTask { get; set; }
        public string IDTask { get; set; }
        public string Link { get; set; }
        public string LibDispName { get; set; }
        public string NewTasks { get; set; }
        public string InProgressTasks { get; set; }
        public string TerminateTasks { get; set; }
        public string IDCard { get; set; }

    }
    public class Message
    {
        public string MessageType { get; set; }
        public string AssignedTo { get; set; }
    }

    public class FirstMessageType : Message
    {
        
        public string TitleTask { get; set; }
        public string IDTask { get; set; }
        public string Link { get; set; }
        public string IDCard { get; set; }
        public string LibDispName { get; set; }
    }
    public class SecondMessageType : Message
    {
        
        public string NewTasks { get; set; }
        public string InProgressTasks { get; set; }
        public string Link { get; set; }
        public string TerminateTasks { get; set; }
    }
    public class ThirdMessageType : Message
    {

        public string IDTask { get; set; }
        public string TitleTask { get; set; }
        public string IDCard { get; set; }
        public string LibDispName { get; set; }
        public string Link { get; set; }

    }

}
