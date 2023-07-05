using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;

namespace BSoupSpeechGenerator;

public class SceneGenerator
{

    private Random random;
    private List<Image> backgrounds;
    private Dictionary<string, List<List<Image>>> speakers; //string name of character, List<List<Image>>> a list containing a list of all a characters outfits
    private SpeechBubbleGenerator speechBubbleGenerator;

    public SceneGenerator()
    {
        Console.WriteLine("Initializing Scene Generator");
        random = new();

        backgrounds = new List<Image>();
        
        var backgroundpaths = Directory.GetFiles(Path.Join(AppContext.BaseDirectory, "Resources", "Backgrounds"));
        foreach (var background in backgroundpaths)
        {
            backgrounds.Add(Image.Load(background));
        }

        speakers = new Dictionary<string, List<List<Image>>>();
        var speakersDir = Directory.EnumerateDirectories(Path.Join(AppContext.BaseDirectory, "Resources", "Speakers"));
        foreach (var dir in speakersDir)
        {
            string name = Path.GetFileName(dir);
            List<List<Image>> charOutfits = new();
            var characterdir = Directory.EnumerateDirectories(dir);
            foreach (var chardir in characterdir)
            {
                List<Image> outfitImages = new();
                var outfitPaths = Directory.GetFiles(chardir);
                foreach (var outfitImagePath in outfitPaths)
                {
                    outfitImages.Add(Image.Load(outfitImagePath));
                }
                charOutfits.Add(outfitImages);
            }
            speakers.Add(name, charOutfits);
        }

        speechBubbleGenerator = new SpeechBubbleGenerator();
        Console.WriteLine("Scene Generator Loaded");
    }

    public FileStream GenerateMP4NoIntermediary(List<(string, string)> script) //<(author, message)>
    { 
        
        var result = generateFrames(script);
        var animation = result.Item1;
        var keyFrames = result.Item2;

        var videoFrameSource = new RawVideoPipeSource(CreateRawFrames(animation, keyFrames))
        {
            FrameRate = 30
        };

        Console.WriteLine("beginning conversion");
        string tempPath = $"temp_{DateTime.Now.ToFileTime()}.mp4";
        try
        {
            FFMpegArguments
                .FromPipeInput(videoFrameSource)
                .AddFileInput(Path.Join(AppContext.BaseDirectory, "Resources", "Music", "flameOfLoveLoop.ogg"))
                .OutputToFile(tempPath, true, options => options
                .ForceFormat("mp4")
                .WithSpeedPreset(Speed.UltraFast)
                .WithCustomArgument("-vf mpdecimate")
                .UsingShortest()
                .WithFastStart()
                )
                .ProcessSynchronously();
        }
        catch (System.Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        var outStream = new FileStream(tempPath, FileMode.Open);
        outStream.Position = 0;
        return outStream;
    }

    private (Image<Rgba32>, List<int>) generateFrames(List<(string, string)> script) {
        Console.WriteLine("beginning generation");
        Dictionary<string, (int, int)> authorCharacterDict = new(); //<author, (character, outfit)>
        List<int> chosenCharacters = new();
        foreach (var statementTuple in script)
        {
            if (authorCharacterDict.ContainsKey(statementTuple.Item1)) continue;

            int charInt;
            while (true)
            {
                charInt = random.Next(speakers.Count);
                if (!chosenCharacters.Contains(charInt)) break;
            }
            int outfitInt = random.Next(speakers.Values.ToList()[charInt].Count);
            authorCharacterDict.Add(statementTuple.Item1, (charInt, outfitInt));
            chosenCharacters.Add(charInt);
        }
        int backgroundSelection = random.Next(backgrounds.Count);
        Image background = backgrounds[backgroundSelection];

        var animation = new Image<Rgba32>(background.Width, background.Height);
        List<int> keyFrames = new();
        foreach (var statementTuple in script)
        {
            var spriteTuple = authorCharacterDict[statementTuple.Item1];
            var partialAnimation = generateStatement(statementTuple.Item1, statementTuple.Item2, backgroundSelection, spriteTuple.Item1, spriteTuple.Item2, random.Next(speakers.Values.ToList()[spriteTuple.Item1][spriteTuple.Item2].Count));
            foreach (var frame in partialAnimation.Frames)
            {
                animation.Frames.AddFrame(frame);
            }
            keyFrames.Add(animation.Frames.Count-2);
        }
        animation.Frames.RemoveFrame(0);

        return (animation, keyFrames);
    }

    private IEnumerable<ImageSharpFrameWrapper<Rgba32>> CreateRawFrames(Image<Rgba32> animation, List<int> keyFrames)
    {
        int delay = 0;
        for (int i = 0; i < animation.Frames.Count; i++)
        {
            yield return new ImageSharpFrameWrapper<Rgba32>(animation.Frames.CloneFrame(i));
            if(keyFrames.Contains(i) && delay < 60) {
                i--;
                delay++;
            } else {
                delay = 0;
            }
        }
    }

    private Image<Rgba32> generateStatement(string author, string message, int backgroundSelection, int characterSelection, int outfitSelection, int moodSelection)
    {
        Image background = backgrounds[backgroundSelection];
        Image sprite = speakers.Values.ToList()[characterSelection][outfitSelection][moodSelection];

        Image<Rgba32> canvas = new(background.Width, background.Height);
        canvas.Mutate(c => c.DrawImage(background, 1f).DrawImage(sprite, new Point(background.Width / 2 - sprite.Width / 2, background.Height - sprite.Height), 1f));

        var statementAnimation = new Image<Rgba32>(background.Width, background.Height);

        var speechList = speechBubbleGenerator.GenerateAnimatedList(author, message);
        foreach (var speechFrame in speechList)
        {
            var frame = canvas.Clone(c => c.DrawImage(speechFrame, new Point(0, canvas.Height - speechFrame.Height), 1f));
            statementAnimation.Frames.AddFrame(frame.Frames.RootFrame);
        }
        statementAnimation.Frames.RemoveFrame(0);
        return statementAnimation;
    }

}