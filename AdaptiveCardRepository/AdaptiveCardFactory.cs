using AdaptiveCards;
using Microsoft.Bot.Schema;
using ProactiveBot.Models;
using ProactiveBot.Models.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProactiveBot.AdaptiveCardRepository
{
    public static class AdaptiveCardFactory
    {

        public static Attachment CreateAdaptiveCardForSubmitAttachment(ThirdMessageType message)
        {
            var messageCopy = (ThirdMessageType)message.Clone();
            var messageCopyReject = (ThirdMessageType)message.Clone();


            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0)); 
            AdaptiveCard subCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));
            AdaptiveCard subCardReject = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));

            subCard.Body.Add(new AdaptiveTextInput()
            {
                Id = "ApproveComment",
                Placeholder = "Коментувати",
                IsMultiline = true
            });
            subCard.Actions.Add(
             new AdaptiveSubmitAction
             {
                 Title = "Погодити",
                 Data = ThirdMessageType.messageApprove(messageCopy)
             }
            );
           
            subCardReject.Body.Add(new AdaptiveTextInput()
            {
                Id = "Comment",
                Placeholder = "Коментувати",
                IsMultiline = true

            });
            subCardReject.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"Це поле обов'язкове до заповнення!",
                Size = AdaptiveTextSize.Small,
                Color = AdaptiveTextColor.Attention,
                Wrap = true
            });
            subCardReject.Actions.Add(
             new AdaptiveSubmitAction
             {
                 Title = "Відхилити",
                 Data = ThirdMessageType.messageReject(messageCopyReject)
             }
          );
            if (message.MessageType == "1")
            {


                card.Body.Add(new AdaptiveTextBlock()
                {
                    Text = $"Вам назначенна задача  **№{message.IDTask}**",
                    Size = AdaptiveTextSize.ExtraLarge,
                    Wrap = true
                });
                card.Body.Add(new AdaptiveTextBlock()
                {
                    Text = $"Назва задачі: {message.TitleTask}",
                    Size = AdaptiveTextSize.Large,
                    Wrap = true
                });
                card.Body.Add(new AdaptiveTextBlock()
                {
                    Text = $"Біблиотека: {message.LibDispName}",
                    Size = AdaptiveTextSize.Medium,
                    Wrap = true

                });
                card.Body.Add(new AdaptiveTextBlock()
                {
                    Text = $"[Карточка **№{message.IDCard}**]({ message.Link})",
                    Size = AdaptiveTextSize.Medium,
                    Wrap = true
                });
              
            }
            else
            {
                card.Body.Add(new AdaptiveTextBlock()
                {
                    Text = $"Задача **№{message.IDTask}**",
                    Size = AdaptiveTextSize.ExtraLarge,
                    Wrap = true
                });
                card.Body.Add(new AdaptiveTextBlock()
                {
                    Text = $"Назва задачі: {message.TitleTask}",
                    Size = AdaptiveTextSize.Large,
                    Wrap = true
                });
                card.Body.Add(new AdaptiveTextBlock()
                {
                    Text = $"Біблиотека: {message.LibDispName}",
                    Size = AdaptiveTextSize.Medium,
                    Wrap = true

                });
                card.Body.Add(new AdaptiveTextBlock()
                {
                    Text = $"[Карточка **№{ message.IDCard }**]({ message.Link})",
                    Size = AdaptiveTextSize.Default,
                    Wrap = true
                });
               
                card.Body.Add(new AdaptiveTextBlock()
                {
                    Text = $"***Задача буде протермінована через 24 години.***",
                    Size = AdaptiveTextSize.Default,
                    Wrap = true
                });

            }
            card.Actions.Add(
               new AdaptiveShowCardAction
               {
                   Card = subCard,
                   Title = "Погодити"
               }
            );
            card.Actions.Add(
              new AdaptiveShowCardAction
              {
                  Card = subCardReject,
                  Title = "Відхилити"
              }
           );
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = card,
            };
            return adaptiveCardAttachment;
        }

        public static Attachment CreateAdaptiveCardWaitingAttachment(ThirdMessageType message)
        {

            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));


            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"Задача **№{message.IDTask}**",
                Size = AdaptiveTextSize.ExtraLarge,
                Wrap = true
            });
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"Назва задачі: {message.TitleTask}",
                Size = AdaptiveTextSize.Large,
                Wrap = true
            });
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $" Бібліотека: {message.LibDispName}",
                Size = AdaptiveTextSize.Default,
                Wrap = true
            });
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"[Карточка **№{ message.IDCard }**]({ message.Link})",
                Size = AdaptiveTextSize.Default,
                Wrap = true
            });


            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"Очікую підтвердження дії від LSDocs!*",
                Size = AdaptiveTextSize.Large,
                Color = AdaptiveTextColor.Accent,
                Wrap = true
            }); ;


            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = card,
            };
            return adaptiveCardAttachment;
        }
        public static Attachment CreateAdaptiveCardAfterSubmitAttachment(ThirdMessageType message, string Aproved, string comment = "")
        {
            Aproved = Aproved == "Approved" ? "Погоджено" : Aproved == "Rejected" ? "Відхилено" : "Помилка";
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));


            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"Задача **№{message.IDTask}**",
                Size = AdaptiveTextSize.ExtraLarge,
                Wrap = true
            });
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"Назва задачі: {message.TitleTask}",
                Size = AdaptiveTextSize.Large,
                Wrap = true
            });
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $" Бібліотека: {message.LibDispName}",
                Size = AdaptiveTextSize.Default,
                Wrap = true
            });
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"[Карточка **№{ message.IDCard }**]({ message.Link})",
                Size = AdaptiveTextSize.Default,
                Wrap = true
            });
         
            if (!string.IsNullOrEmpty(comment.Trim()))
            {
                card.Body.Add(new AdaptiveTextBlock()
                {
                    Text = $"Коментар: {comment}",
                    Size = AdaptiveTextSize.Default,
                    Wrap = true
                });
            }
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"Статус : {Aproved}",
                Size = AdaptiveTextSize.Medium,
                Wrap = true
            });


            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = card,
            };
            return adaptiveCardAttachment;
        }
        public static Attachment CreateAdaptiveCardCommentRequiredAttachment(ThirdMessageType message)
        {
            var messageCopy = (ThirdMessageType)message.Clone();
            var messageCopyReject = (ThirdMessageType)message.Clone();

            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));
            AdaptiveCard subCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));
            AdaptiveCard subCardReject = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));

            subCardReject.Body.Add(new AdaptiveTextInput()
            {
                Id = "Comment",
                Placeholder = "Коментувати",
                IsMultiline = true,

            });
            subCardReject.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"Це поле обов'язкове до заповнення!",
                Size = AdaptiveTextSize.Small,
                Color = AdaptiveTextColor.Attention,
                Wrap = true
            });
            subCardReject.Actions.Add(
             new AdaptiveSubmitAction
             {
                 Title = "Відхилити",
                 Data = ThirdMessageType.messageReject(messageCopyReject)
             }
          );
            subCard.Body.Add(new AdaptiveTextInput()
            {
                Id = "ApproveComment",
                Placeholder = "Коментувати",
                IsMultiline = true
            });
            subCard.Actions.Add(
             new AdaptiveSubmitAction
             {
                 Title = "Погодити",
                 Data = ThirdMessageType.messageApprove(messageCopy)
             }
          );
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"Задача **№{message.IDTask}**",
                Size = AdaptiveTextSize.ExtraLarge,
                Wrap = true
            });
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"Назва задачі: {message.TitleTask}",
                Size = AdaptiveTextSize.Large,
                Wrap = true
            });
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $" Бібліотека: {message.LibDispName}",
                Size = AdaptiveTextSize.Default,
                Wrap = true
            });
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"[Карточка **№{ message.IDCard }**]({ message.Link})",
                Size = AdaptiveTextSize.Default,
                Wrap = true
            });
           
            if (message.MessageType == "3")
            {
                card.Body.Add(new AdaptiveTextBlock()
                {
                    Text = $"***Задача буде протермінована через 24 години.***",
                    Size = AdaptiveTextSize.Default,
                    Wrap = true
                });
            }
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"**Поле коментар обов'язкове**",
                Size = AdaptiveTextSize.Medium,
                Color = AdaptiveTextColor.Attention,
                Wrap = true
            });

            card.Actions.Add(
               new AdaptiveShowCardAction
               {
                   Card = subCard,
                   Title = "Погодити"
               }
            );
            card.Actions.Add(
              new AdaptiveShowCardAction
              {
                  Card = subCardReject,
                  Title = "Відхилити"
              }
           );
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = card,
            };
            return adaptiveCardAttachment;
        }
        public static Attachment CreateAdaptiveCardAlreadySubmitAttachment(ThirdMessageType message, string Aproved)
        {
            Aproved = Aproved == "Approved" ? "Погоджено" : Aproved == "Rejected" ? "Відхилено" : "Помилка";

            var messageCopy = (ThirdMessageType)message.Clone();
            var messageCopyReject = (ThirdMessageType)message.Clone();

            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));

            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"Задача **№{message.IDTask}**",
                Size = AdaptiveTextSize.ExtraLarge,
                Wrap = true
            });
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"Назва задачі: {message.TitleTask}",
                Size = AdaptiveTextSize.Large,
                Wrap = true
            });
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $" Бібліотека: {message.LibDispName}",
                Size = AdaptiveTextSize.Default,
                Wrap = true
            });
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"[Карточка **№{ message.IDCard }**]({ message.Link})",
                Size = AdaptiveTextSize.Default,
                Wrap = true
            });
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"***Дії по цій задачі уже було виконано в LSDocs!***",
                Size = AdaptiveTextSize.Large,
                Color = AdaptiveTextColor.Warning,
                Wrap = true
            });
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"Статус : {Aproved}",
                Size = AdaptiveTextSize.Medium,
                Wrap = true
            });

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = card,
            };
            return adaptiveCardAttachment;
        }

        public static Attachment CreateAdaptiveCardFirstTypeAttachment(ThirdMessageType message)
        {

            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));

            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"Бібліотека: {message.LibDispName}",
                Size = AdaptiveTextSize.Medium,
                Wrap = true

            });
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"Для Вас назначена нова задача в LSDocs **№{message.IDTask}**",
                Size = AdaptiveTextSize.Large,
                Wrap = true
            });
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"Назва задачі: {message.TitleTask}",
                Size = AdaptiveTextSize.Medium,
                Wrap = true
            });
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"[Карточка **№{message.IDCard}**]({ message.Link})",
                Size = AdaptiveTextSize.Medium,
                Wrap = true
            });
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = card,
            };
            return adaptiveCardAttachment;
        }
        public static Attachment CreateAdaptiveCardSecondTypeAttachment(string displayName, SecondMessageType message)
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));


            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"**{displayName}**, статус задач у Вашому власному кабінеті LSDOCS :",
                Size = AdaptiveTextSize.Medium,
                Wrap = true
            });
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"* Нових задач: {message.NewTasks} \n\n" + $"*Задач в роботі:  {message.InProgressTasks} \n\n" + $"* Протермінованих задач: {message.NewTasks}\n\n",
                Size = AdaptiveTextSize.Medium,
                Wrap = true
            });
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"[Перейти до власного кабінету]({message.Link})\n",
                Size = AdaptiveTextSize.Medium,
                Wrap = true

            });
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = card,
            };
            return adaptiveCardAttachment;
        }
        public static Attachment CreateAdaptiveCardThirdTypeAttachment(ThirdMessageType message)
        {
            var messageCopy = (ThirdMessageType)message.Clone();

            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"Задача  **№{message.IDTask}**",
                Size = AdaptiveTextSize.Large,
                Wrap = true
            });
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"Назва задачі: {message.TitleTask}",
                Size = AdaptiveTextSize.Medium,
                Wrap = true
            });
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $" Бібліотека: {message.LibDispName}",
                Size = AdaptiveTextSize.Default,
                Wrap = true
            });
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"[Карточка **№{ message.IDCard }**]({ message.Link})",
                Size = AdaptiveTextSize.Default,
                Wrap = true
            });
           
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"***Задача буде протермінована через 24 години.***",
                Size = AdaptiveTextSize.Default,
                Wrap = true
            });
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = card,
            };
            return adaptiveCardAttachment;
        }

    }
}
