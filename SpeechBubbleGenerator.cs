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

    public SpeechBubbleGenerator()
    {
        collection = new();
        collection.Add($"Resources{Path.DirectorySeparatorChar}YunusH.ttf"); //"YunusH"
        collection.Add($"Resources{Path.DirectorySeparatorChar}myriad.ttf"); // "Myriad Pro"
        collection.Add($"Resources{Path.DirectorySeparatorChar}Twemoji.Mozilla.ttf"); //"Twemoji Mozilla"

        var dialogueFamily = collection.Get("Myriad Pro");
        var nameFamily = collection.Get("YunusH");
        var emojiFamily = collection.Get("Twemoji Mozilla");

        dialogueFont = dialogueFamily.CreateFont(30f);
        dialogueFontOptions = new(dialogueFont)
        {
            Origin = new Point(90, 90),
            WrappingLength = 1150,
            FallbackFontFamilies = new[] { emojiFamily },
            ColorFontSupport = ColorFontSupport.MicrosoftColrFormat
        };

        nameFont = nameFamily.CreateFont(45f);
        nameFontOptions = new(nameFont)
        {
            Origin = new Point(25, 10),
            WrappingLength = 420,
            FallbackFontFamilies = new[] { emojiFamily },
            ColorFontSupport = ColorFontSupport.MicrosoftColrFormat
        };



        Image textBox = Image.Load($"Resources{Path.DirectorySeparatorChar}textbox.png");
        Image nameBox = Image.Load($"Resources{Path.DirectorySeparatorChar}namebox2.png");
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

    public List<Image<Rgba32>> GenerateAnimatedList(string author, string message) {
        List<Image<Rgba32>> returnList = new();
        String buildingMessage = "";
        var messageInfo = new StringInfo(message);
        Image<Rgba32> canvas = speechBubbleBase.Clone();
        canvas.Mutate(c => c.DrawText(nameFontOptions, author, Color.White));

        for (int i = 0; i < messageInfo.LengthInTextElements; i++)
        {
            buildingMessage += messageInfo.SubstringByTextElements(i, 1);
            canvas.Mutate(c => c.DrawText(dialogueFontOptions, buildingMessage, Color.White));
            returnList.Add(canvas.Clone());
        }
        return returnList;
    }
}