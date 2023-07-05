using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;
using System.Globalization;

namespace BSoupSpeechGenerator;

class SpeechBubbleGenerator
{

    private FontCollection collection;
    private Font nameFont;
    private TextOptions nameFontOptions;
    private Font dialogueFont;
    private TextOptions dialogueFontOptions;
    private Image<Rgba32> speechBubbleBase;
    private readonly int dialogueWrapLength = 1150;

    public SpeechBubbleGenerator()
    {
        collection = new();
        collection.Add(Path.Join(AppContext.BaseDirectory, "Resources", "YunusH.ttf")); //"YunusH"
        collection.Add(Path.Join(AppContext.BaseDirectory, "Resources", "myriad.ttf")); // "Myriad Pro"
        collection.Add(Path.Join(AppContext.BaseDirectory, "Resources", "Twemoji.Mozilla.ttf")); //"Twemoji Mozilla"

        var dialogueFamily = collection.Get("Myriad Pro");
        var nameFamily = collection.Get("YunusH");
        var emojiFamily = collection.Get("Twemoji Mozilla");

        dialogueFont = dialogueFamily.CreateFont(30f);
        dialogueFontOptions = new(dialogueFont)
        {
            Origin = new Point(90, 90),
            // WrappingLength = dialogueWrapLength,
            FallbackFontFamilies = new[] { emojiFamily },
            ColorFontSupport = ColorFontSupport.MicrosoftColrFormat,
        };

        nameFont = nameFamily.CreateFont(45f);
        nameFontOptions = new(nameFont)
        {
            Origin = new Point(25, 10),
            WrappingLength = 420,
            FallbackFontFamilies = new[] { emojiFamily },
            ColorFontSupport = ColorFontSupport.MicrosoftColrFormat
        };

        Image textBox = Image.Load(Path.Join(AppContext.BaseDirectory, "Resources","textbox.png"));
        Image nameBox = Image.Load(Path.Join(AppContext.BaseDirectory, "Resources","namebox2.png"));
        speechBubbleBase = new Image<Rgba32>(textBox.Width, 267);

        speechBubbleBase.Mutate(c => c.DrawImage(textBox, new Point(0, 30), .75f));
        speechBubbleBase.Mutate(c => c.DrawImage(nameBox, new Point(0, 0), 1f));
    }
    public Image<Rgba32> GenerateBubbleImage(string author, string message)
    {
        Image<Rgba32> canvas = speechBubbleBase.Clone();

        canvas.Mutate(c => c.DrawText(dialogueFontOptions, message, Color.White));
        canvas.Mutate(c => c.DrawText(nameFontOptions, author, Color.White));

        return canvas;
    }

    public List<Image<Rgba32>> GenerateAnimatedList(string author, string message)
    {
        List<Image<Rgba32>> returnList = new();
        string buildingMessage = "";
        var messageInfo = new StringInfo(message);
        Image<Rgba32> canvas = speechBubbleBase.Clone();
        canvas.Mutate(c => c.DrawText(nameFontOptions, author, Color.White));

        for (int i = 0; i < messageInfo.LengthInTextElements; i++)
        {

            if (i > 0 && messageInfo.SubstringByTextElements(i - 1, 1) == " " && messageInfo.SubstringByTextElements(i, 1) != " ")
            {
                string word = "";
                for (int j = 0; (i + j < messageInfo.LengthInTextElements) && (messageInfo.SubstringByTextElements(i + j, 1) != " "); j++)
                {
                    word += messageInfo.SubstringByTextElements(i + j, 1);
                    if (TextMeasurer.Measure(buildingMessage + word, dialogueFontOptions).Width >= dialogueWrapLength)
                    {
                        buildingMessage += '\n';
                        break;
                    }
                }
            }
            else
            {
                if (TextMeasurer.Measure(buildingMessage + messageInfo.SubstringByTextElements(i, 1), dialogueFontOptions).Width > dialogueWrapLength)
                {
                    buildingMessage += '\n';
                }
            }

            if (i < messageInfo.LengthInTextElements - 2 && TextMeasurer.CountLines(buildingMessage + messageInfo.SubstringByTextElements(i, 2), dialogueFontOptions) == 5)
            {
                for (int j = 0; j < 30; j++)
                {
                    returnList.Add(returnList.Last().Clone());
                }
                canvas = speechBubbleBase.Clone();
                canvas.Mutate(c => c.DrawText(nameFontOptions, author, Color.White));
                buildingMessage = "";
            }

            buildingMessage += messageInfo.SubstringByTextElements(i, 1);
            canvas = speechBubbleBase.Clone();
            canvas.Mutate(c => c.DrawText(nameFontOptions, author, Color.White));
            canvas.Mutate(c => c.DrawText(dialogueFontOptions, buildingMessage, Color.White));
            returnList.Add(canvas.Clone());
        }
        return returnList;
    }
}