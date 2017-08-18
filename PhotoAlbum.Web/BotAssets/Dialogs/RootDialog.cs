using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using PhotoAlbum.Data;

namespace PhotoAlbum.Web.Bot.Dialogs
{
    #pragma warning disable 1998

    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private const string ShowMeFriends = "(1) Show me pictures of my friends";
        private const string ShowMeTags = "(2) Show pictures from a tag";
        //private const string SearchForPictures = "(3) Let me search for pictures by their description";

        private readonly IDictionary<string, string> _options = new Dictionary<string, string>
        {
            { "1", ShowMeFriends },
            { "2", ShowMeTags },
            //{ "3", SearchForPictures }
        };

        [field: NonSerializedAttribute()]
        private ImageRepository _imageRepo;

        public async Task StartAsync(IDialogContext context)
        {
            /* Wait until the first message is received from the conversation and call MessageReceviedAsync 
             *  to process that message. */
            context.Wait(this.MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            /* When MessageReceivedAsync is called, it's passed an IAwaitable<IMessageActivity>. To get the message,
             *  await the result. */
            var message = await result;

            await this.SendWelcomeMessageAsync(context);
        }

        private async Task SendWelcomeMessageAsync(IDialogContext context)
        {
            await context.PostAsync("Hi, I'm the Intelligent Photo Album bot. Let's get started!");

            await this.DisplayOptionsAsync(context);
        }

        public async Task DisplayOptionsAsync(IDialogContext context)
        {
            PromptDialog.Choice<string>(
                context,
                this.ProcessSelectedOptionAsync,
                this._options.Keys,
                "What photos would you like to see?",
                "Ooops, what you wrote is not a valid option, please try again",
                3,
                PromptStyle.PerLine,
                this._options.Values);
        }

        public async Task ProcessSelectedOptionAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;

            switch (message)
            {
                case "1":
                    context.Call(new PicturesOfFriendDialog(), this.PicturesOfFriendsDialogResumeAfter);
                    break;
                case "2":
                    context.Call(new PicturesOfTagDialog(), this.PicturesOfTagsDialogResumeAfter);
                    break;
                //case "3":
                //    // TODO: Search image descriptions
                //    break;
            }
        }

        private async Task PicturesOfFriendsDialogResumeAfter(IDialogContext context, IAwaitable<string> result)
        {
            var replyMessage = context.MakeMessage();
            _imageRepo = new ImageRepository(SettingsHelper.DatabaseConnectionString());
            var attachments = new List<Attachment>();

            try
            {
                var name = await result;
                var pictures = _imageRepo.GetByPersonName(name).Take(3).ToList();
                if (pictures.Any())
                {
                    attachments.AddRange(pictures.Select(GetAttachment));

                    // The Attachments property allows you to send and receive images and other content.
                    replyMessage.Attachments = attachments;
                    replyMessage.Text = $"I found a few pictures of {name} that were recently added...";

                    await context.PostAsync(replyMessage);

                    await this.DisplayOptionsAsync(context);
                }
                else
                {
                    await context.PostAsync(
                        $"Hmmm.... I can't seem to find any pictures of {name} :(  Maybe try another name?");

                    await this.DisplayOptionsAsync(context);
                }
            }
            catch (TooManyAttemptsException)
            {
                await context.PostAsync("I'm sorry, I'm having issues understanding you. Let's try again.");

                await this.DisplayOptionsAsync(context);
            }
        }

        private async Task PicturesOfTagsDialogResumeAfter(IDialogContext context, IAwaitable<string> result)
        {
            var replyMessage = context.MakeMessage();
            _imageRepo = new ImageRepository(SettingsHelper.DatabaseConnectionString());
            var attachments = new List<Attachment>();

            try
            {
                var tag = await result;
                var pictures = _imageRepo.GetByTag(tag).Take(3).ToList();
                if (pictures.Any())
                {
                    attachments.AddRange(pictures.Select(GetAttachment));

                    // The Attachments property allows you to send and receive images and other content.
                    replyMessage.Attachments = attachments;
                    replyMessage.Text = $"I found a few pictures of '{tag}' that were recently added...";

                    await context.PostAsync(replyMessage);

                    await this.DisplayOptionsAsync(context);
                }
                else
                {
                    await context.PostAsync(
                        $"Well... I can't seem to find any pictures tagged with {tag} :(  Let's search for something else, ok?");

                    await this.DisplayOptionsAsync(context);
                }
            }
            catch (TooManyAttemptsException)
            {
                await context.PostAsync("I'm sorry, I'm having issues understanding you. Let's try again.");

                await this.DisplayOptionsAsync(context);
            }
        }

        private static Attachment GetAttachment(DTO.Image image)
        {
            return new Attachment
            {
                Name = image.ImageName,
                ContentType = "image/jpeg",
                ContentUrl = $"{AbsoluteUrl.HomeIndex}/cloud/uploads/{image.ImageName}"
            };
        }
    }
}