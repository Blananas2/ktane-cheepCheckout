using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;

public class cheepCheckoutScript : MonoBehaviour
{

    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMRuleSeedable Ruleseed;

    public KMSelectable[] MainButtons;
    public KMSelectable[] OtherButtons; //disp, left, right, clear, submit
    public TextMesh[] MainTexts;
    public TextMesh[] OtherTexts;
    public Color[] Colors; //white, red, yellow, green

    //KMAudio.KMAudioRef soundEffect;
    /// <summary> All available bird names, sorted by first letter, with the first name in each being Ruleseed 1. </summary>
    private readonly string[][] birdNames = new string[27][] {
        new string[5] { "Auklet", "Albatross", "Antbird", "Apalis", "Aracari" },
        new string[5] { "Bluebird", "Bird-of-Paradise", "Bittern", "Bananaquit", "Booby" },
        new string[5] { "Chickadee", "Crane", "Cuckoo", "Courser", "Chachalaca" },
        new string[5] { "Dove", "Darter", "Duck", "Drongo", "Doradito" },
        new string[4] { "Egret", "Emu", "Eagle", "Eremomela" },
        new string[5] { "Finch", "Flamingo", "Frigatebird", "Firefinch", "Friarbird" },
        new string[5] { "Godwit", "Grouse", "Guan", "Gull", "Go-Away Bird" },
        new string[4] { "Hummingbird", "Hoatzin", "Hornbill", "Heron" },
        new string[4] { "Ibis", "Inezia", "Iora", "Indigobird" },
        new string[4] { "Jay", "Jacamar", "Jacana", "Jacobin" },
        new string[5] { "Kinglet", "Kiwi", "Kagu", "Kingfisher", "Kea" },
        new string[5] { "Loon", "Lark", "Lyrebird", "Leaftosser", "Limpkin" },
        new string[5] { "Magpie", "Mallard", "Motmot", "Murrelet", "Morepork" },
        new string[4] { "Nuthatch", "Nightingale", "Noddy", "Nightjar" },
        new string[4] { "Oriole", "Ostrich", "Oilbird", "Owl" },
        new string[5] { "Pipit", "Pardalote", "Pheasant", "Potoo", "Pigeon" },
        new string[2] { "Quail", "Quetzal" },
        new string[5] { "Raven", "Rhea", "Robin", "Roadrunner", "Riflebird" },
        new string[5] { "Shrike", "Scrubbird", "Seedsnipe", "Seriema", "Shearwater" },
        new string[5] { "Thrush", "Turkey", "Turaco", "Tern", "Tūī" },
        new string[2] { "Umbrellabird", "Ural Owl" },
        new string[3] { "Vireo", "Vulture", "Vanga" },
        new string[5] { "Warbler", "Wren", "Woodpecker", "Wagtail", "Weaver" },
        new string[2] { "Xantus’s Hummingbird", "Xavier's Greenbul" },
        new string[3] { "Yellowlegs", "Yellowthroat", "Yuhina" },
        new string[3] { "Zigzag Heron", "Zebra Dove", "Zino's Petrel" },
        new string[1] { "[Unicorn Bastard]" }
    };

    /// <summary> All base bird prices, in order for Ruleseed 1. </summary>
    private readonly List<int> birdPrices = new List<int> { 359, 633, 199, 250, 901, 690, 527, 912, 410, 893, 728, 123, 377, 99, 314, 141, 904, 800, 420, 260, 1, 967, 369, 551, 201, 753};

    /// <summary> All 27 possible bird pitches in the order of the Ruleseed 1 manual. </summary>
    private readonly int[][] birdPitches = new int[27][] {
        new int[3] { 1, 1, 0 }, 
        new int[3] { 2, 0, 1 }, 
        new int[3] { 1, 1, 2 }, 
        new int[3] { 1, 0, 2 }, 
        new int[3] { 0, 0, 1 }, 
        new int[3] { 1, 1, 1 }, 
        new int[3] { 0, 1, 1 }, 
        new int[3] { 0, 1, 0 }, 
        new int[3] { 2, 0, 0 }, 
        new int[3] { 0, 2, 0 }, 
        new int[3] { 0, 2, 1 }, 
        new int[3] { 1, 2, 1 }, 
        new int[3] { 2, 1, 2 }, 
        new int[3] { 0, 0, 2 }, 
        new int[3] { 1, 0, 1 }, 
        new int[3] { 0, 2, 2 }, 
        new int[3] { 1, 2, 2 }, 
        new int[3] { 2, 1, 1 }, 
        new int[3] { 2, 1, 0 }, 
        new int[3] { 2, 2, 1 }, 
        new int[3] { 2, 0, 2 }, 
        new int[3] { 1, 0, 0 }, 
        new int[3] { 1, 2, 0 }, 
        new int[3] { 0, 1, 2 }, 
        new int[3] { 2, 2, 2 }, 
        new int[3] { 2, 2, 0 }, 
        new int[3] { 0, 0, 0 },
    };

    /// <summary> Now that Ruleseed is present, represents the name of the 27 possible birds, one for each letter in order. </summary>
    private string[] ruleseededBirdNames = new string[27];

    /// <summary> Now that Ruleseed is present, is used as an index to know the shuffled order of the Bird Pitches. </summary>
    private List<int> ruleseededBirdPitchesOrder;

    /// <summary> Now that Ruleseed is present, represents the new Bird Prices in order, with added variance. </summary>
    private List<int> ruleseededBirdPrices = new List<int>();

    /// <summary> Does Ruleseed ask for the presence of Cheap Checkout instead of a specific indicator? </summary>
    private bool ruleseedUnicornIsCheapCheckout;
    // <summary> Does Ruleseed ask for a Lit indicator? </summary>
    private bool ruleseedUnicornIndicatorIsLit;
    // <summary> Which Indicator does ruleseed check for? </summary>
    private int ruleseedUnicornIndicatorType;

    private readonly string[] possibleIndicators = new string[11]{"SND", "CLR", "CAR", "IND", "FRQ", "SIG", "NSA", "MSA", "TRN", "BOB", "FRK"};


    /// <summary> Index of the 27 possible birds, with Unicorn being the 27th </summary>
    private int[] numberArray = new int[27] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26 };

    private readonly List<string> mainNumbers = new List<string> { ".01", ".05", ".10", ".25", " 1 ", " 5 ", "10", "25" };
    
    private readonly List<string> pitchNames = new List<string> { "Low", "Medium", "High" };
    int selectedBird = 0;
    int pressedButton = 0;
    int unicornPressCount = 0;
    int birdPrice = 0;
    int currentPrice = 0;
    int customerPrice = 0;
    int answerPrice = 0;
    bool validPrice = true;
    bool entering = false;
    bool hasUnicornBird = false;
    bool unicorn = false;
    bool waiting = false;


    // Data easily readable for Souvenir now that we have ruleseed
    private string SouvenirBirdsPitches;

    //Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    private CheepCheckoutSettings Settings = new CheepCheckoutSettings();
    bool RandomizeButtons;

    void Awake()
    {
        moduleId = moduleIdCounter++;

        ModConfig<CheepCheckoutSettings> modConfig = new ModConfig<CheepCheckoutSettings>("CheepCheckoutSettings");
       //Read from the settings file, or create one if one doesn't exist
       Settings = modConfig.Settings;
       //Update the settings file incase there was an error during read
       modConfig.Settings = Settings;
       Debug.LogFormat("<Cheep Checkout #{0}> RandomizeButtons: {1}", moduleId, Settings.RandomizeButtons);

        foreach (KMSelectable mButton in MainButtons)
        {
            KMSelectable pressedMButton = mButton;
            mButton.OnInteract += delegate () { mButtonPress(pressedMButton); return false; };
        }

        foreach (KMSelectable oButton in OtherButtons)
        {
            KMSelectable pressedOButton = oButton;
            oButton.OnInteract += delegate () { oButtonPress(pressedOButton); return false; };
        }
    }

    // Use this for initialization
    void Start()
    {
        numberArray.Shuffle();

        ManageRuleseed();
        ManageRuleseededSouvenir();

        birdPrice = ruleseededBirdPrices[numberArray[0]] + ruleseededBirdPrices[numberArray[1]] + ruleseededBirdPrices[numberArray[2]] + ruleseededBirdPrices[numberArray[3]] + ruleseededBirdPrices[numberArray[4]];
        Debug.Log("<Cheep Checkout>" + ruleseededBirdNames[numberArray[0]] + " + " + ruleseededBirdNames[numberArray[1]] + " + " + ruleseededBirdNames[numberArray[2]] + " + " + ruleseededBirdNames[numberArray[3]] + " + " + ruleseededBirdNames[numberArray[4]] + " = " + birdPrice);

        for (int k = 0; k < 5; k++)
        {
            int[] _birdPitch = birdPitches[ruleseededBirdPitchesOrder[numberArray[k]]];
            Debug.LogFormat("[Cheep Checkout #{0}] Bird {1}: {2}, {3}, {4} ({5}, costs ${6})", moduleId, k + 1, pitchNames[_birdPitch[0]], pitchNames[_birdPitch[1]], pitchNames[_birdPitch[2]], ruleseededBirdNames[numberArray[k]], dollar(ruleseededBirdPrices[numberArray[k]]));
        }

        if (numberArray[0] == 26 || numberArray[1] == 26 || numberArray[2] == 26 || numberArray[3] == 26 || numberArray[4] == 26)
        {
            hasUnicornBird = true;
            Debug.Log("<Cheep Checkout>" + "UNICORN BIRD DETECTED");
        }

        customerPrice = UnityEngine.Random.Range(5, 20) * 100;
        OtherTexts[0].text = "$" + dollar(customerPrice);

        answerPrice = customerPrice - birdPrice;



        // Check for Unicorn
        if (ruleseedUnicornIsCheapCheckout)
        {
            unicorn = Bomb.GetSolvableModuleNames().Contains("CheapCheckoutModule");
        }
        else
        {
            if (ruleseedUnicornIndicatorIsLit)
            {
                unicorn = Bomb.IsIndicatorOn((Indicator)ruleseedUnicornIndicatorType);
            }
            else
            {
                unicorn = Bomb.IsIndicatorOff((Indicator)ruleseedUnicornIndicatorType);
            }
        }

        if (hasUnicornBird && unicorn)
        {
            Debug.Log("<Cheep Checkout>" + "UNICORN IN EFFECT");
            Debug.LogFormat("[Cheep Checkout #{0}] Unicorn rule applies, please repeatedly slap the customer.", moduleId);
        }
        else
        {
            Debug.LogFormat("[Cheep Checkout #{0}] All amounts total to ${1}.", moduleId, dollar(birdPrice));
            Debug.LogFormat("[Cheep Checkout #{0}] Customer paid ${1}.", moduleId, dollar(customerPrice));
            if (answerPrice <= 0)
            {
                validPrice = false;
                Debug.LogFormat("[Cheep Checkout #{0}] Customer has not paid enough money, submitting nothing required.", moduleId);
            }
            else
            {
                Debug.LogFormat("[Cheep Checkout #{0}] Correct change is ${1}.", moduleId, dollar(answerPrice));
            }
        }
    }

    void ManageRuleseed()
    {
        MonoRandom Rng = Ruleseed.GetRNG();

        // Those values are used as-is for ruleseed 1 or shuffled afterwards
        ruleseededBirdPitchesOrder = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26 };

        // This specifically contains only 26 values, because we only shuffle the usable ones;
        ruleseededBirdPrices = birdPrices;

        // Ruleseed 1, initialize default values
        if (Rng.Seed == 1)
        {
            for (int i = 0; i < 27; i ++)
            {
                ruleseededBirdNames[i] = birdNames[i][0];
            }

            // Add the Unicorn Bastard's values back
            ruleseededBirdPrices.Insert(26, 0);

            // Manage unicorn => Lit BOB present
            ruleseedUnicornIsCheapCheckout = false;
            ruleseedUnicornIndicatorIsLit = true;
            ruleseedUnicornIndicatorType = 9;
            return;
        }


        Debug.LogFormat("[Cheep Checkout #{0}] Using Ruleseed {1}:", moduleId, Rng.Seed);

        // Following the order of the manual:
        // First the Pitches using a single FisherYates
        // Then the Prices using a single FisherYates
        // Then 26 times:
        //      One FisherYates for the letter
        //      One rnd.next(-10, 11) for price variance
        // Then one rnd.next(0, 10) for Unicorn condition
        // If != 10
        //      One rnd.next(0, 2) for lit/unlit choice
        //      One rnd.next(0, 11) for Indicator selection

        Rng.ShuffleFisherYates(ruleseededBirdPitchesOrder);

        ruleseededBirdPrices = birdPrices;
        Rng.ShuffleFisherYates(ruleseededBirdPrices);

        string[] namesForThisLetter;
        for (int i = 0; i < 26; i ++)
        {
            namesForThisLetter = birdNames[i];
            Rng.ShuffleFisherYates(namesForThisLetter);
            ruleseededBirdNames[i] = namesForThisLetter[0];

            ruleseededBirdPrices[i] += Rng.Next(-10, 11);
            if (ruleseededBirdPrices[i] < 1) ruleseededBirdPrices[i] = 1;
        }

        // Unicorn Bastard is not present in the manual, so it must be handled separately here
        ruleseededBirdNames[26] = birdNames[26][0];
        ruleseededBirdPrices.Insert(26, 0);

        // Pitch is already managed as we want to randomize the unicorn's cry too, so no need to care here

        Debug.Log("<Cheep Checkout>" + ruleseededBirdNames.Join(" // "));
        Debug.Log("<Cheep Checkout>" + ruleseededBirdPrices.Join(" // "));
        Debug.Log("<Cheep Checkout>" + ruleseededBirdPitchesOrder.Select(x => birdPitches[x].Join()).Join(" // "));

        if (Rng.Next(0, 10) == 0)
        {
            ruleseedUnicornIsCheapCheckout = true;
            Debug.Log("<Cheep Checkout>" + "UNICORN IS CHEAP CHECKOUT PRESENT");
        }
        else
        {
            ruleseedUnicornIsCheapCheckout = false;
            ruleseedUnicornIndicatorIsLit = Rng.Next(0, 2) == 0;
            ruleseedUnicornIndicatorType = Rng.Next(0, 11);

            Debug.Log("<Cheep Checkout>" + "UNICORN IS " + (ruleseedUnicornIndicatorIsLit ? "LIT " : "UNLIT ") + possibleIndicators[ruleseedUnicornIndicatorType] + " PORT");
        }
    }

    /// <summary> Souvenir uses a constant string for its data; so we're just gonna replace it in the format it needs with our new data </summary>
    void ManageRuleseededSouvenir()
    {
        char[] souvenirPitchConversion = new char[3] { 'L', 'M', 'H'};

        SouvenirBirdsPitches = "";

        for (int i = 0; i < 27; i ++)
        {
            SouvenirBirdsPitches += ruleseededBirdNames[i] + '=' + birdPitches[ruleseededBirdPitchesOrder[i]].Select(x => souvenirPitchConversion[x]).Join("") + ';';
        }

        Debug.LogFormat("<Cheep Checkout #{0}> SOUVENIR PITCHES DATA STRING: {1}", moduleId, SouvenirBirdsPitches);
    }

    string dollar (int n) {
        if (n % 100 < 10) {
            return n/100 + ".0" + n%100;
        } else {
            return n/100 + "." + n%100;
        }
    }

    void mButtonPress(KMSelectable pressedMButton)
    {
        pressedMButton.AddInteractionPunch();
        if (moduleSolved == false)
        {
            entering = true;
            for (int j = 0; j < 8; j++)
            {
                if (pressedMButton == MainButtons[j])
                {
                    pressedButton = j;
                }
            }
            if (MainTexts[pressedButton].text == ".01")
            {
                currentPrice += 1;
                Audio.PlaySoundAtTransform("dove", transform);
            }
            else if (MainTexts[pressedButton].text == ".05")
            {
                currentPrice += 5;
                Audio.PlaySoundAtTransform("dove", transform);
            }
            else if (MainTexts[pressedButton].text == ".10")
            {
                currentPrice += 10;
                Audio.PlaySoundAtTransform("dove", transform);
            }
            else if (MainTexts[pressedButton].text == ".25")
            {
                currentPrice += 25;
                Audio.PlaySoundAtTransform("dove", transform);
            }
            else if (MainTexts[pressedButton].text == " 1 ")
            {
                currentPrice += 100;
                Audio.PlaySoundAtTransform("falcon", transform);
            }
            else if (MainTexts[pressedButton].text == " 5 ")
            {
                currentPrice += 500;
                Audio.PlaySoundAtTransform("falcon", transform);
            }
            else if (MainTexts[pressedButton].text == "10")
            {
                currentPrice += 1000;
                Audio.PlaySoundAtTransform("falcon", transform);
            }
            else if (MainTexts[pressedButton].text == "25")
            {
                currentPrice += 2500;
                Audio.PlaySoundAtTransform("falcon", transform);
            }

            OtherTexts[0].color = Colors[2];
            Debug.Log("<Cheep Checkout>" + currentPrice);

            OtherTexts[0].text = "$" + dollar(currentPrice);

            if (Settings.RandomizeButtons) {
                mainNumbers.Shuffle();
                MainTexts[0].text = mainNumbers[0];
                MainTexts[1].text = mainNumbers[1];
                MainTexts[2].text = mainNumbers[2];
                MainTexts[3].text = mainNumbers[3];
                MainTexts[4].text = mainNumbers[4];
                MainTexts[5].text = mainNumbers[5];
                MainTexts[6].text = mainNumbers[6];
                MainTexts[7].text = mainNumbers[7];
            }
        }
    }

    void oButtonPress(KMSelectable pressedOButton)
    {
        pressedOButton.AddInteractionPunch();
        if (moduleSolved == false)
        {
            if (pressedOButton == OtherButtons[0]) // DISPLAY
            {
                StartCoroutine(PlaySounds());
            }
            else if (pressedOButton == OtherButtons[1]) //LEFT
            {
                if (selectedBird != 0)
                {
                    selectedBird -= 1;
                }
                else
                {
                    Audio.PlaySoundAtTransform("african-grey-parrot", transform);
                }
                Debug.Log("<Cheep Checkout>" + "Selected Bird: " + selectedBird);
            }
            else if (pressedOButton == OtherButtons[2]) //RIGHT
            {
                if (selectedBird != 4)
                {
                    selectedBird += 1;
                }
                else
                {
                    Audio.PlaySoundAtTransform("african-grey-parrot", transform);
                }
                Debug.Log("<Cheep Checkout>" + "Selected Bird: " + selectedBird);
            }
            else if (pressedOButton == OtherButtons[3]) //CLEAR
            {
                OtherTexts[0].color = Colors[0];
                currentPrice = 0;
                entering = false;
                OtherTexts[0].text = "$" + dollar(customerPrice);
                Audio.PlaySoundAtTransform("crow", transform);
            }
            else if (pressedOButton == OtherButtons[4]) //SUBMIT
            {
                if (unicorn == false)
                {
                    if (entering == false)
                    {
                        if (validPrice == true)
                        {
                            Debug.LogFormat("[Cheep Checkout #{0}] Customer slapped, but they have paid enough money, now they're pissed, here's a strike.", moduleId);
                            GetComponent<KMBombModule>().HandleStrike();
                            Audio.PlaySoundAtTransform("crow", transform);
                            StartCoroutine(RedText());
                        }
                        else
                        {
                            Debug.LogFormat("[Cheep Checkout #{0}] Customer slapped successfully.", moduleId);
                            if (!waiting) {
                                Audio.PlaySoundAtTransform("crow", transform);
                                StartCoroutine(OneSecond());
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("<Cheep Checkout>" + currentPrice + " | " + answerPrice);
                        if (currentPrice == answerPrice)
                        {
                            Debug.LogFormat("[Cheep Checkout #{0}] Correct amount of change given, module solved.", moduleId);
                            moduleSolved = true;
                            StartCoroutine(CorrectAnswer());
                        }
                        else
                        {
                            Debug.LogFormat("[Cheep Checkout #{0}] Incorrect amount of change given (${1}), module striked.", moduleId, dollar(currentPrice));
                            Audio.PlaySoundAtTransform("crow", transform);
                            GetComponent<KMBombModule>().HandleStrike();
                            StartCoroutine(RedText());
                        }
                    }
                } else
                {
                    unicornPressCount += 1;
                    Debug.LogFormat("[Cheep Checkout #{0}] Ow!", moduleId);
                    if (unicornPressCount >= 15)
                    {
                        moduleSolved = true;
                        StartCoroutine(UnicornSolve());
                    } else
                    {
                        Audio.PlaySoundAtTransform("crow", transform);
                    }
                }
            }
        }
    }

    IEnumerator PlaySounds()
    {
        int[] _birdPitches = birdPitches[ruleseededBirdPitchesOrder[numberArray[selectedBird]]];

        for (int i = 0; i < 3; i ++)
        {
            Debug.Log("<Cheep Checkout>" + _birdPitches[i]);

            switch (_birdPitches[i])
            {
                case 0: Audio.PlaySoundAtTransform("low1", transform); break;
                case 1: Audio.PlaySoundAtTransform("med1", transform); break;
                case 2: Audio.PlaySoundAtTransform("high1", transform); break;
            }

            if (i != 2)
            { yield return new WaitForSeconds(0.68f); }
            else
            { yield return null; }
        }
    }

    IEnumerator OneSecond()
    {
        waiting = true;
        Audio.PlaySoundAtTransform("robin", transform);
        for (int i = 0; i < 2; i++)
        {
            OtherTexts[0].text = "ONE SECOND";
            yield return new WaitForSeconds(0.375f);
            OtherTexts[0].text = "ONE SECOND.";
            yield return new WaitForSeconds(0.375f);
            OtherTexts[0].text = "ONE SECOND..";
            yield return new WaitForSeconds(0.375f);
            OtherTexts[0].text = "ONE SECOND...";
            yield return new WaitForSeconds(0.375f);
        }
        customerPrice += UnityEngine.Random.Range(5,20) * 100;
        OtherTexts[0].text = "$" + dollar(customerPrice);
        Debug.LogFormat("[Cheep Checkout #{0}] New price is ${1}.", moduleId, dollar(customerPrice));
        answerPrice = customerPrice - birdPrice;
        Debug.Log("<Cheep Checkout>" + answerPrice);
        if (answerPrice <= 0)
        {
            validPrice = false;
            Debug.LogFormat("[Cheep Checkout #{0}] Customer has still not paid enough money, submitting nothing required... again.", moduleId);
        } else
        {
            validPrice = true;
        }
        waiting = false;
    }

    IEnumerator RedText()
    {
        OtherTexts[0].color = Colors[1];
        yield return new WaitForSeconds(1);
        OtherTexts[0].color = Colors[0];
        currentPrice = 0;
        entering = false;
        OtherTexts[0].text = "$" + dollar(customerPrice);
    }

    IEnumerator CorrectAnswer()
    {
        OtherTexts[0].color = Colors[3];
        Audio.PlaySoundAtTransform("Rooster", transform);
        yield return new WaitForSeconds(1.645f);
        GetComponent<KMBombModule>().HandlePass();
        MainTexts[0].text = "W";
        MainTexts[1].text = "E";
        MainTexts[2].text = "L";
        MainTexts[3].text = "L";
        MainTexts[4].text = "D";
        MainTexts[5].text = "O";
        MainTexts[6].text = "N";
        MainTexts[7].text = "E";
    }

    IEnumerator UnicornSolve()
    {
        Debug.LogFormat("[Cheep Checkout #{0}] The customer is now dead, module solved.", moduleId);
        OtherTexts[0].color = Colors[3];
        OtherTexts[0].text = "*DEAD*";
        Audio.PlaySoundAtTransform("Rooster", transform);
        yield return new WaitForSeconds(1.645f);
        GetComponent<KMBombModule>().HandlePass();
        MainTexts[0].text = "U";
        MainTexts[1].text = "N";
        MainTexts[2].text = "I";
        MainTexts[3].text = "C";
        MainTexts[4].text = "O";
        MainTexts[5].text = "R";
        MainTexts[6].text = "N";
        MainTexts[7].text = "<3";
    }

    //twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} birds [Cycles through all the birds] | !{0} bird <#> [Goes to the specified bird] | !{0} press <button> [Presses the specified button] | !{0} mash [Mashes the submit button] | Valid buttons are submit, clear, and 1-8 representing the value buttons in reading order";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        if (Regex.IsMatch(command, @"^\s*mash\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(command, @"^\s*slapfest\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            for(int i = 0; i < 15; i++)
            {
                OtherButtons[4].OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
            if (moduleSolved)
                yield return "solve";
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*slap\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(command, @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            OtherButtons[4].OnInteract();
            if (moduleSolved)
                yield return "solve";
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*birds\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            while(selectedBird != 0)
            {
                OtherButtons[1].OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
            yield return new WaitForSeconds(0.75f);
            OtherButtons[0].OnInteract();
            yield return new WaitForSeconds(3.0f);
            for (int i = 0; i < 4; i++)
            {
                yield return "trycancel Bird cycling cancelled due to a cancel request";
                OtherButtons[2].OnInteract();
                yield return new WaitForSeconds(0.75f);
                OtherButtons[0].OnInteract();
                yield return new WaitForSeconds(3.0f);
            }
            while (selectedBird != 0)
            {
                OtherButtons[1].OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
            yield break;
        }
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*press\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            if (parameters.Length == 2)
            {
                if (Regex.IsMatch(parameters[1], @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                {
                    yield return null;
                    OtherButtons[4].OnInteract();
                }
                else if (Regex.IsMatch(parameters[1], @"^\s*clear\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                {
                    yield return null;
                    OtherButtons[3].OnInteract();
                }
                else if (Regex.IsMatch(parameters[1], @"^\s*1\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                {
                    yield return null;
                    MainButtons[0].OnInteract();
                }
                else if (Regex.IsMatch(parameters[1], @"^\s*2\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                {
                    yield return null;
                    MainButtons[1].OnInteract();
                }
                else if (Regex.IsMatch(parameters[1], @"^\s*3\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                {
                    yield return null;
                    MainButtons[2].OnInteract();
                }
                else if (Regex.IsMatch(parameters[1], @"^\s*4\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                {
                    yield return null;
                    MainButtons[3].OnInteract();
                }
                else if (Regex.IsMatch(parameters[1], @"^\s*5\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                {
                    yield return null;
                    MainButtons[4].OnInteract();
                }
                else if (Regex.IsMatch(parameters[1], @"^\s*6\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                {
                    yield return null;
                    MainButtons[5].OnInteract();
                }
                else if (Regex.IsMatch(parameters[1], @"^\s*7\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                {
                    yield return null;
                    MainButtons[6].OnInteract();
                }
                else if (Regex.IsMatch(parameters[1], @"^\s*8\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                {
                    yield return null;
                    MainButtons[7].OnInteract();
                }
            }
            yield break;
        }
        if (Regex.IsMatch(parameters[0], @"^\s*bird\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            if (parameters.Length == 2)
            {
                if (Regex.IsMatch(parameters[1], @"^\s*1\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                {
                    yield return null;
                    int start = selectedBird;
                    for (int i = start; i > 0; i--)
                    {
                        OtherButtons[1].OnInteract();
                        yield return new WaitForSeconds(0.1f);
                    }
                    yield return new WaitForSeconds(0.75f);
                    OtherButtons[0].OnInteract();
                }
                else if (Regex.IsMatch(parameters[1], @"^\s*2\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                {
                    yield return null;
                    int start = selectedBird;
                    if (start < 1)
                    {
                        OtherButtons[2].OnInteract();
                    }
                    else if (start > 1)
                    {
                        for (int i = start; i > 1; i--)
                        {
                            OtherButtons[1].OnInteract();
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                    yield return new WaitForSeconds(0.75f);
                    OtherButtons[0].OnInteract();
                }
                else if (Regex.IsMatch(parameters[1], @"^\s*3\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                {
                    yield return null;
                    int start = selectedBird;
                    if (start < 2)
                    {
                        for (int i = start; i < 2; i++)
                        {
                            OtherButtons[2].OnInteract();
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                    else if (start > 2)
                    {
                        for (int i = start; i > 2; i--)
                        {
                            OtherButtons[1].OnInteract();
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                    yield return new WaitForSeconds(0.75f);
                    OtherButtons[0].OnInteract();
                }
                else if (Regex.IsMatch(parameters[1], @"^\s*4\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                {
                    yield return null;
                    int start = selectedBird;
                    if (start > 3)
                    {
                        OtherButtons[1].OnInteract();
                    }
                    else if (start < 3)
                    {
                        for (int i = start; i < 3; i++)
                        {
                            OtherButtons[2].OnInteract();
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                    yield return new WaitForSeconds(0.75f);
                    OtherButtons[0].OnInteract();
                }
                else if (Regex.IsMatch(parameters[1], @"^\s*5\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                {
                    yield return null;
                    int start = selectedBird;
                    for (int i = start; i < 4; i++)
                    {
                        OtherButtons[2].OnInteract();
                        yield return new WaitForSeconds(0.1f);
                    }
                    yield return new WaitForSeconds(0.75f);
                    OtherButtons[0].OnInteract();
                }
            }
            yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        if (unicorn)
        {
            yield return ProcessTwitchCommand("mash");
            yield break;
        }
        yield return ProcessTwitchCommand("press clear");
        while (!validPrice)
        {
            yield return ProcessTwitchCommand("slap");
            while (waiting) yield return true;
        }
        int totalleft = answerPrice;
        while (totalleft > 0)
        {
            if (totalleft >= 2500)
            {
                MainButtons[mainNumbers.IndexOf("25")].OnInteract();
                totalleft -= 2500;
            }
            else if (totalleft >= 1000)
            {
                MainButtons[mainNumbers.IndexOf("10")].OnInteract();
                totalleft -= 1000;
            }
            else if (totalleft >= 500)
            {
                MainButtons[mainNumbers.IndexOf(" 5 ")].OnInteract();
                totalleft -= 500;
            }
            else if (totalleft >= 100)
            {
                MainButtons[mainNumbers.IndexOf(" 1 ")].OnInteract();
                totalleft -= 100;
            }
            else if (totalleft >= 25)
            {
                MainButtons[mainNumbers.IndexOf(".25")].OnInteract();
                totalleft -= 25;
            }
            else if (totalleft >= 10)
            {
                MainButtons[mainNumbers.IndexOf(".10")].OnInteract();
                totalleft -= 10;
            }
            else if (totalleft >= 5)
            {
                MainButtons[mainNumbers.IndexOf(".05")].OnInteract();
                totalleft -= 5;
            }
            else if (totalleft >= 1)
            {
                MainButtons[mainNumbers.IndexOf(".01")].OnInteract();
                totalleft -= 1;
            }
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.1f);
        OtherButtons[4].OnInteract();
    }

    class CheepCheckoutSettings
    {
        public bool RandomizeButtons = true;
    }

    static Dictionary<string, object>[] TweaksEditorSettings = new Dictionary<string, object>[]
    {
        new Dictionary<string, object>
        {
            { "Filename", "CheepCheckoutSettings.json" },
            { "Name", "Cheep Checkout Settings" },
            { "Listings", new List<Dictionary<string, object>>{
                new Dictionary<string, object>
                {
                    { "Key", "RandomizeButtons" },
                    { "Text", "Randomize Buttons" },
                    { "Description", "Determines whether the buttons get randomized on press." }
                },
            } }
        }
    };
}
