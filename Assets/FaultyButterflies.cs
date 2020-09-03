using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KModkit;
using System.Linq;
using System;
using rnd = UnityEngine.Random;
public class FaultyButterflies : MonoBehaviour {
    public KMSelectable[] butterflies;
    public Sprite[] butterflySprites;
    public SpriteRenderer[] butterlflyRenderers;
    string[] commonNames = new string[] { "Eastern Tiger Swallowtail", "Ornythion Swallowtail", "Spicebush Swallowtail",  "Pipevine Swallowtail", "Polydamas Swallowtail", "Zebra Swallowtail", "Chiricahua White", "Checkered White", "Orange-Barred Sulphur", "Labrador Sulphur", "Sleepy Orange", "Desert Orangetip", "White M Hairstreak", "Great Purple Hairstreak", "Colorado Hairstreak", "Common Buckeye", "Milbert's Tortoiseshell", "Eastern Comma", "Lustrous Copper", "Blue Copper", "American Copper", "Grizzled Skipper", "Eastern Tailed-Blue", "Theona Checkerspot", "Little Metalmark", "Zebra Heliconian", "Tiger Mimic Queen", "Regal Fritillary", "Diana Fritillary", "Crimson Patch", "White Peacock", "Banded Peackock", "Malachite", "Rusty-Tipped Page", "Painted Lady", "Mexican Bluewing", "Viceroy", "Monarch", "Red-Spotted Purple", "Ruddy Daggerwing", "Litte Wood Satyr", "Guava Skipper"};
    string[] scientificNames = new string[] {"Papilo glaucus", "Papilo ornythion", "Papilo troilus", "Battus philenor", "Battus polydamus", "Eurytides marcellus", "Neophasia terlootii", "Pontia protodice", "Phoebis philea", "Colias nastes", "Eurema nicippe", "Anthocharis cethura", "Parrhasius m-alubm", "Atlides halesus", "Hypaurotis crysalus", "Junonia coenia", "Nymphalis milberti", "Polygonia comma", "Lycaenea cupriea", "Lycaenea heteronea", "Lycaenea phlaeas", "Pyrgus centaureae", "Everes comyntas",  "Chlosyne theona", "Calephelis virginiensis", "Heliconius charithonia", "Lycorea halia", "Speyeria idalia", "Speyeria diana", "Chlosyne janais", "Anartia jatrophae", "Anartia fatima", "Siproeta stelenes", "Siproeta epaphus", "Vanessa cardui", "Myscelia ethusa", "Limenitis archippus", "Danaus plexippus", "Limenitis arthemis", "Marpesia petreus", "Megisto cymela", "Phocides palemon"};
    string[] names;
    List<int> butterflyIndices = new List<int>();
    List<string> namesUsed = new List<string>();
    List<int> butterflyIndicesOrdered = new List<int>();
    int stage;
    public KMBombModule module;
    public KMBombInfo bomb;
    public KMAudio sound;
    int moduleId;
    static int moduleIdCounter = 1;
    bool solved;
    private void Awake()
    {
        moduleId = moduleIdCounter++;
        foreach (KMSelectable i in butterflies)
        {
            KMSelectable butterfly = i;
            butterfly.OnInteract += delegate { pressButterfly(butterfly); return false;};
        }
    }
    void Start () {
        int candidateNumber = 0;
        if (bomb.GetSerialNumberNumbers().Last() % 2 == 1)
        {
            names = commonNames;
        }
        else
        {
            names = scientificNames;
        }
        for (int i = 0; i < butterflies.Length; i++)
        {
            do
            {
              candidateNumber = rnd.Range(0, 42);
            } while (butterflyIndices.Contains(candidateNumber));
            butterflyIndices.Add(candidateNumber);

            namesUsed.Add(names[butterflyIndices[i]]);
            butterlflyRenderers[i].sprite = butterflySprites[butterflyIndices[i]];
        }
        Debug.LogFormat("[Faulty Butterflies #{0}] The butterflies in the order they appear on the module are {1}.", moduleId, namesUsed.Join(", "));
        namesUsed.Sort();
        Debug.LogFormat("[Faulty Butterflies #{0}] The butterflies in the order they should be pressed are {1}.", moduleId, namesUsed.Join(", "));
        foreach (string i in namesUsed)
        {
            butterflyIndicesOrdered.Add(Array.IndexOf(names, i));
        }
	}

	void pressButterfly (KMSelectable butterfly) {
		if (!solved)
        {
            butterfly.AddInteractionPunch();
            Debug.LogFormat("[Faulty Butterflies #{0}] You pressed {1}.", moduleId, names[butterflyIndices[Array.IndexOf(butterflies, butterfly)]]);
            if (butterflyIndices[Array.IndexOf(butterflies, butterfly)] != butterflyIndicesOrdered[stage])
            {
                module.HandleStrike();
                Debug.LogFormat("[Faulty Butterflies #{0}] That was incorrect. Strike!", moduleId);
                butterflyIndices.Clear();
                butterflyIndicesOrdered.Clear();
                namesUsed.Clear();
                stage = 0;
                Start();
            }   
            else if (stage == butterflies.Length - 1)
            {
                module.HandlePass();
                Debug.LogFormat("[Faulty Butterflies #{0}] That was correct. Module solved.", moduleId);
                sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
                solved = true;
            }
            else
            {
                stage++;
            }
        }
	}
#pragma warning disable 414
    private string TwitchHelpMessage = "use '!{0} 123456' to press the butterflies in reading order.";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant();
        string validcmds = "123456";
        if (command.Contains(' '))
        {
            yield return "sendtochaterror @{0}, invalid command.";
            yield break;
        }
        else
        {
            for (int i = 0; i < command.Length; i++)
            {
                if (!validcmds.Contains(command[i]))
                {
                    yield return "sendtochaterror Invalid command.";
                    yield break;
                }
            }
            for (int i = 0; i < command.Length; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    if (command[i] == validcmds[j])
                    {
                        yield return null;
                        butterflies[j].OnInteract();
                    }
                }
            }
        }
    }
    IEnumerator TwitchHandleForcedSolve()
    {
        foreach (int i in butterflyIndicesOrdered)
        {
            for (int j = 0; j < butterflyIndices.Count; j++)
            {
                if (butterflyIndices[j] == i)
                {
                    yield return null;
                    butterflies[j].OnInteract();
                }
            }
        }
    }
}
