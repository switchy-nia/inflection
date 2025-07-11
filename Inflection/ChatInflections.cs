using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
// using Serilog;
namespace Inflection
{
    public class Cooldown
    {
        int count = 1;
        int cooldown = -1;
        public bool CanExecute { get { return count >= cooldown; } }
        public Cooldown(int cooldown)
        {
            this.cooldown = cooldown;
        }
        public void Tick()
        {
            count++;
        }
        public void Reset()
        {
            // For spacing sensibility. (Minimum)
            count = 1;
        }
    }

    public class Inflections
    {
        // Patterns used to parse out the tokens.
        const string PUNCTUATION_PATTERN = @"([\.\?\!\[\]\{\}\,\(\)]|INFOPENEMOTE|INFCLOSEEMOTE)";
        // This emoji should support the following basic text emojis
        const string EMOJI_REGEX = @"^[:;cDxX><\-\,)CP][:;><\-3)CWwdpP]*[:;cDxX><\-\,3)CWwdpP]$";

        Profile profile;
        Regex punctuationRegex = new Regex(PUNCTUATION_PATTERN);
        Regex emoji_regex = new Regex(EMOJI_REGEX);
        Regex mute_regex = new Regex(@"([\(\*].*?[\*\)])");

        private List<(Regex, string, int)> patterns = new List<(Regex, string, int)>();


        public Inflections(Profile new_profile)
        {
            profile = new_profile;
            foreach (var pattern in profile.Patterns)
            {
                patterns.Add((new Regex(pattern.Pattern, RegexOptions.IgnoreCase), pattern.Replacement, pattern.Chance));
            }
        }

        public bool SetToInflect()
        {
            return
                profile.CompelledSpeechEnabled ||
                profile.TicksEnabled ||
                profile.WordReplacementEnabled ||
                profile.StutterEnabled ||
                profile.SentenceStartEnabled ||
                profile.SentenceEndingEnabled;
        }
        public string Speak(String input)
        {

            StringBuilder output = new StringBuilder();
            Random rand = new Random();

            //Log.Debug($"speak {input} - start");
            // Note Mute is a special case where we just strip everything that isn't OOC or emote
            // Therefore, we can do an early return once it is completed.
            if (profile.MuteEnabled)
            {
                foreach (Match match in mute_regex.Matches(input))
                {
                    output.Append(match.Value);
                }
                return output.ToString();
            }

            // First tokenize it.
            // Simple method of ensuring all punctuation and words have a space between them, then splitting by space.
            // First we need to handle the RP tag '*', this is mostly for formatting purposes later (only one set is supported for now).
            var tokens = Regex.Replace(input, @"\*(.*)\*", "INFOPENEMOTE $1 INFCLOSEEMOTE");
            tokens = tokens.Replace("*", "");
            tokens = punctuationRegex.Replace(tokens, @" $1 ");
            List<string> words = tokens.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

            Cooldown tickCooldown = new Cooldown(profile.TickCooldown);
            Cooldown stutterCooldown = new Cooldown(profile.StutterCooldown);
            bool rp_emote_chat = false;
            bool ooc_chat = false;

            //Log.Debug($"Looping over words with {words.Count}");
            for (int i = 0; i < words.Count; i++)
            {
                string word = words[i];
                if (emoji_regex.IsMatch(word))
                {
                    //Log.Debug($"{full_word} is an emoji, skipping");
                    output.Append(word);
                    output.Append(' ');
                    continue;
                }
                if (punctuationRegex.IsMatch(word))
                {
                    if (!rp_emote_chat && word.Contains("INFOPENEMOTE"))
                        rp_emote_chat = true;
                    else if (rp_emote_chat && word.Contains("INFCLOSEEMOTE"))
                        rp_emote_chat = false;
                    if (!ooc_chat && word.Contains('('))
                        ooc_chat = true;
                    else if (ooc_chat && word.Contains(')'))
                        ooc_chat = false;

                    output.Append(handlePunctuation(word));
                    output.Append(' ');
                    continue;
                }


                // In if we are currently handling OOC or emote messages, we append and skip processing.
                if (rp_emote_chat || ooc_chat)
                {
                    output.Append(word);
                }
                else
                {
                    output.Append(ProcessWord(rand, word, tickCooldown, stutterCooldown));
                }

                output.Append(' ');
            }

            var final = output.ToString();
            if (profile.SentenceStartEnabled)
            {
                final = SentenceStart(rand, final);
            }

            if (profile.SentenceEndingEnabled)
            {
                final = SentenceEnding(rand, final);
            }

            if (profile.PatternsEnabled)
            {
                final = ApplyPatterns(final, rand);
            }
            // undo the tokenization. 
            // And restore the message formatting now that we're done messing with it.
            final = Regex.Replace(final, @"([\(\[\{])\s+(\w)", "$1$2");
            final = Regex.Replace(final, @"\s+([\)\]\}\,\.\!\?])", "$1");

            final = Regex.Replace(final, @"([\w\.\!\?])\s+([\)\]\}\,\.\!\?])", "$1$2");
            final = Regex.Replace(final, @"INFOPENEMOTE\s+", "*");
            final = Regex.Replace(final, @"\s+INFCLOSEEMOTE", "*");
            // TODO: Handle quotes
            return final.Trim();
        }

        /// <summary>
        /// Handles the punctuation of individual word so that things like *word, are handled properly.
        /// </summary>
        /// <param name="punctuation"></param>
        /// <returns>tuple that indicates a prefix punctuation, the word content, the postfix punctuation</returns>
        private string handlePunctuation(string punctuation)
        {
            switch (profile.VoiceType)
            {

                case VoiceVolume.Small:
                    punctuation = punctuation.Replace(".", "...");
                    punctuation = punctuation.Replace("!", "..!");
                    punctuation = punctuation.Replace("?", "..?");
                    break;

                case VoiceVolume.Big:
                    punctuation = punctuation.Replace("!", "!!");
                    punctuation = punctuation.Replace("?", "!?");
                    punctuation = punctuation.Replace(".", "!");
                    break;
            }
            return punctuation;
        }

        // Takes the word and processes it based on the specific words
        private string ProcessWord(Random rand, string word, Cooldown tickCooldown, Cooldown stutterCooldown)
        {
            // By default it will be empty
            switch (profile.VoiceType)
            {
                case VoiceVolume.Small:
                    word = word.ToLower();
                    break;
                case VoiceVolume.Big:
                    word = word.ToUpper();
                    break;
            }


            // Forced speech trumps all other speech and returns immediately, no further processing required.
            if (profile.CompelledSpeechEnabled)
            {
                //Log.Debug($"Process {word} using forced speech and early returning");
                int index = rand.Next(profile.CompelledSpeechWords.Count);
                return profile.CompelledSpeechWords.ElementAt(index);
            }

            // If this is found in a pronouns list, change it.
            if (profile.WordReplacementEnabled)
            {
                var key = word.ToLower();
                if (profile.WordReplacement.ContainsKey(key))
                {
                    //Log.Debug($"Process {word} using forced pronouns");
                    word = profile.WordReplacement[key];
                }
            }

            if (profile.StutterEnabled)
            {
                if (stutterCooldown.CanExecute)
                { //Log.Debug($"Process {word} and making it stutter");
                    if (rand.Next(100) < profile.StutterChance)
                    {
                        string stutter = "";
                        // Randomize it with a slightly more drammatic stutter
                        int max_stutters = rand.Next(profile.MaxStutterSeverity);
                        for (int i = 0; i < 1 + max_stutters; i++)
                        {
                            stutter += word.First() + "-";
                        }
                        word = stutter + word;
                        stutterCooldown.Reset();
                    }
                }
                else
                {
                    stutterCooldown.Tick();
                }
            }

            // Finally if there is an utterance required, add it.
            // The number of ticks here is to respect the maximum portion of a sentence
            // (to prevent RNG from making people have a spasm of utterances unless they want to.)
            if (profile.TicksEnabled)
            {
                if (tickCooldown.CanExecute)
                {

                    if (profile.Ticks.Count == 0)
                    {
                        //Log.Debug($"No verbal ticks found, this should not be possible to set, so there is an error in your configuration.CurrentProfile.");
                    }
                    else if (rand.Next(100) < profile.TickChance)
                    {
                        int index = rand.Next(profile.Ticks.Count);
                        string tick = profile.Ticks.ElementAt(index);
                        word = word + " " + tick;
                    }
                    tickCooldown.Reset();
                }
                else
                {
                    tickCooldown.Tick();
                }
            }

            return word;
        }

        private string SentenceStart(Random rand, string input)
        {
            int index = rand.Next(profile.SentenceStarts.Count);
            return profile.SentenceStarts.ElementAt(index) + " " + input;
        }

        private string SentenceEnding(Random rand, string input)
        {
            int index = rand.Next(profile.SentenceEndings.Count);
            return input.Trim() + " " + profile.SentenceEndings.ElementAt(index);
        }

        private string ApplyPatterns(string input, Random rand)
        {
            StringBuilder output = new StringBuilder(input);
            foreach ((Regex pattern, string replacement, int chance) in patterns)
            {
                foreach (Match match in pattern.Matches(input))
                {
                    if (chance == 100 || rand.Next(100) < chance)
                    {
                        output = output.Replace(match.Value, match.Result(replacement));
                    }
                }
            }
            return output.ToString();
        }
    }
}
