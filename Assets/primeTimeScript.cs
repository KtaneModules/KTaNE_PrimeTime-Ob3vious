using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class primeTimeScript : MonoBehaviour {

	public KMAudio Audio;
	public AudioClip[] sounds;
	public KMBombInfo BombInfo;
	private TextMesh Text;
	public KMSelectable Button;
	public KMBombModule Module;

	private int[] primeselect = new int[4];
	private bool[] thirdstage = new bool[3] { false, false, false };
	private int[] primes = { 2 };
	private int stage = 0;
	private bool continuing = false;

	public static int modID = 1;
	private int currentModID;

	void Awake() {
		currentModID = modID++;
		Text = Button.GetComponentInChildren<TextMesh>();
		Button.OnInteract += delegate ()
		{
			Button.AddInteractionPunch();
			Audio.PlaySoundAtTransform("ButtonPress", Module.transform);
			if (stage == 0)
			{
				//retrieving enough primes
				for (int i = primes.Last(); i < BombInfo.GetTime(); i++)
				{
					for (int j = 0; j < primes.Length && i % primes[j] != 0; j++)
					{
						if (j == primes.Length - 1 && i % primes[j] != 0)
						{
							primes = primes.Concat(new int[] { i }).ToArray();
						}
					}
				}
				if (!primes.Contains((int)BombInfo.GetTime()))
				{
					Debug.LogFormat("[Prime Time #{0}] {1} is not a prime number.", currentModID, (int)BombInfo.GetTime());
					Text.text = "";
					Module.HandleStrike();
					stage = 0;
				}
				else
				{
					stage++;
					Text.text = primeselect[0].ToString();
				}
			}
			else if (stage == 1)
			{
				if ((int)BombInfo.GetTime() % primeselect[0] != 0)
				{
					Debug.LogFormat("[Prime Time #{0}] {1} is not divisible by {2}.", currentModID, (int)BombInfo.GetTime(), primeselect[0]);
					Text.text = "";
					Module.HandleStrike();
					primeselect[0] = primes.Where(x => x < 100 && !primeselect.Contains(x)).ToArray()[Rnd.Range(0, primes.Where(x => x < 100 && !primeselect.Contains(x)).ToArray().Length)];
					stage = 0;
				}
				else
				{
					stage++;
					continuing = true;
					StartCoroutine(stagethree());
				}
			}
			else if (stage == 2)
			{
				bool change = false;
				bool strikepend = false;
				string logging = "as ";
				for (int i = 0; i < 3; i++)
				{
					if (((int)BombInfo.GetTime() % primeselect[i + 1] == 0))
					{
						change = true;
						if (thirdstage[i])
						{
							strikepend = true;
							if (logging == "as ")
								logging += primeselect[i + 1] + " was already selected";
							else
								logging += ", and " + primeselect[i + 1] + " was already selected";
						}
						thirdstage[i] = true;
					}
				}
				if (strikepend || !change)
				{
					if (logging == "as ")
						logging += "none of the given primes were divisors";
					Debug.LogFormat("[Prime Time #{0}] {1} was not a valid time, {2}.", currentModID, (int)BombInfo.GetTime(), logging);
					Text.text = "";
					Module.HandleStrike();
					continuing = false;
					for (int i = 0; i < 4; i++)
					{
						primeselect[i] = primes.Where(x => x < 100 && !primeselect.Contains(x)).ToArray()[Rnd.Range(0, primes.Where(x => x < 100 && !primeselect.Contains(x)).ToArray().Length)];
					}
					thirdstage = new bool[3] { false, false, false };
					stage = 0;
				}
				else if (thirdstage[0] && thirdstage[1] && thirdstage[2])
				{
					stage++;
					continuing = false;
					StartCoroutine(solved());
					Module.HandlePass();
				}
			}
			return false;
		};
		for (int i = 3; i < 100; i++)
		{
			for (int j = 0; j < primes.Length && i % primes[j] != 0; j++)
			{
				if (j == primes.Length - 1 && i % primes[j] != 0)
				{
					primes = primes.Concat(new int[] { i }).ToArray();
				}
			}
		}
	}

	// Use this for initialization
	void Start() {
		Text.text = "";
		stage = 0;
		for (int i = 0; i < primeselect.Length; i++)
		{
			primeselect[i] = primes.Where(x => x < 100 && !primeselect.Contains(x)).ToArray()[Rnd.Range(0, primes.Where(x => x < 100 && !primeselect.Contains(x)).ToArray().Length)];
		}
	}

	// Update is called once per frame
	void Update() {

	}

	private IEnumerator stagethree()
	{
		while (continuing)
		{
			for (int i = 0; i < 3 && continuing; i++)
			{
				Text.text = primeselect[i + 1].ToString();
				yield return new WaitForSeconds(1f);
			}
		}
		Text.text = "";
	}
	private IEnumerator solved()
	{
		int i = 0;
		while (true)
		{
			Text.text = "Prime";
			yield return new WaitForSeconds(1f);
			Text.text = "Time!";
			yield return new WaitForSeconds(1f);
			Text.text = primes[i].ToString();
			yield return new WaitForSeconds(1f);
			i++;
			for (int k = primes.Last(); primes.Length <= i; k++)
			{
				for (int j = 0; j < primes.Length && k % primes[j] != 0; j++)
				{
					if (j == primes.Length - 1 && k % primes[j] != 0)
					{
						primes = primes.Concat(new int[] { k }).ToArray();
					}
				}
			}
		}
	}



#pragma warning disable 414
	private string TwitchHelpMessage = "'!{0} 420' to press the button when the bomb timer has 420 seconds left. Commands can be chained using spaces.";
#pragma warning restore 414
	private IEnumerator ProcessTwitchCommand(string command)
	{ 
		string validCommands = "0123456789 ";
		for (int i = 0; i < command.Length; i++)
		{
			if (!validCommands.Contains(command[i]))
			{
				yield return "sendtochaterror {0}, " + command[i] + " is not a digit.";
				yield break;
			}
		}
		string[] cmds = command.Split(' ');
		for (int i = 0; i < cmds.Length; i++)
		{
			while ((((int)BombInfo.GetTime()).ToString()) != cmds[i].ToString()) { yield return "trycancel Button wasn't pressed due to request to cancel."; }
			Button.OnInteract();
		}
		yield return null;
	}

	IEnumerator TwitchHandleForcedSolve()
	{
		yield return true;
		stage = 3;
		continuing = false;
		StartCoroutine(solved());
		Module.HandlePass();
	}
}
