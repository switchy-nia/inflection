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
        const string IS_DELIM_PATTERN = @"(INF_SPC|INF_TEMP_SPC)";
        const string PUNCTUATION_PATTERN = @"([\.\?\!\[\]\{\}\,\(\)]|INFOPENEMOTE|INFCLOSEEMOTE)";
        // This emoji should support the following basic text emojis
        const string EMOJI_REGEX = @"^[:;cDxX><\-\,)CP][:;><\-3)CWwdpP]*[:;cDxX><\-\,3)CWwdpP]$";

        Profile profile;
        Regex delimiterRegex = new Regex(IS_DELIM_PATTERN);
        Regex punctuationRegex = new Regex(PUNCTUATION_PATTERN);
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
                profile.SentenceEndingEnabled ||
                profile.MuteEnabled ||
                profile.PatternsEnabled ||
                profile.VoiceType != VoiceVolume.Normal;
        }
        public string Speak(String input)
        {
            // There is no point in attempting to modify the chat when there's no options enabled.
            if (!SetToInflect())
            {
                //Plugin.Log.Debug($"Profile does not have any inflections enabled");
                return input;
            }
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
            //Plugin.Log.Debug($"{input}");
            var tokens = Regex.Replace(input, @"\*(.*)\*", "INFOPENEMOTE$1INFCLOSEEMOTE");

            //Plugin.Log.Debug($"{tokens}");
            // for tokenization purposes space gets a special character replacement
            tokens = Regex.Replace(tokens, " ", "INF_SPC");
            //Plugin.Log.Debug($"{tokens}");
            // Then punctuation gets surrounded by a special temporary token "space" for processing
            tokens = punctuationRegex.Replace(tokens, @"INF_TEMP_SPC$1INF_TEMP_SPC");
            //Plugin.Log.Debug($"{tokens}");
            List<string> words = delimiterRegex.Split(tokens).ToList();

            Cooldown tickCooldown = new Cooldown(profile.TickCooldown);
            Cooldown stutterCooldown = new Cooldown(profile.StutterCooldown);
            bool rp_emote_chat = false;
            bool ooc_chat = false;
            //Plugin.Log.Debug($"tokens {words.Count}");
            //Log.Debug($"Looping over words with {words.Count}");
            for (int i = 0; i < words.Count; i++)
            {
                string word = words[i];
                // For some unknown reason, it is possible for empty words to get appended to the list.
                // Skip them entirely.
                if (word.Length == 0)
                {
                    //Plugin.Log.Debug($"{word} is empty");
                    continue;
                }
                // Delimiters need to be untouched so they can be undone at the end.
                if (word.Equals("INF_SPC") || word.Equals("INF_TEMP_SPC"))
                {
                    //Plugin.Log.Debug($"{word} is a delimiter");
                    output.Append(word);
                    continue;
                }
                if (punctuationRegex.IsMatch(word))
                {
                    //Plugin.Log.Debug($"{word} is punctuation");
                    if (!rp_emote_chat && word.Contains("INFOPENEMOTE"))
                        rp_emote_chat = true;
                    else if (rp_emote_chat && word.Contains("INFCLOSEEMOTE"))
                        rp_emote_chat = false;
                    if (!ooc_chat && word.Contains('('))
                        ooc_chat = true;
                    else if (ooc_chat && word.Contains(')'))
                        ooc_chat = false;

                    output.Append(handlePunctuation(word));
                    continue;
                }


                // In if we are currently handling OOC or emote messages, we append and skip processing.
                if (rp_emote_chat || ooc_chat)
                {
                    output.Append(word);
                }
                else
                {
                    //Plugin.Log.Debug($"{word} is processed as normal");
                    output.Append(ProcessWord(rand, word, tickCooldown, stutterCooldown));
                }
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

            // undo the tokenization. 
            // And restore the message formatting now that we're done messing with it.
            final = Regex.Replace(final, @"INFOPENEMOTE", "*");
            final = Regex.Replace(final, @"INFCLOSEEMOTE", "*");
            final = Regex.Replace(final, @"INF_TEMP_SPC", "");
            final = Regex.Replace(final, @"INF_SPC", " ");
            if (profile.PatternsEnabled)
            {
                final = ApplyPatterns(final, rand);
            }
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
            // Essentially, just find the start index and the end index for any "RP symbols" in the string and then ignore those indexes
            var matching_parens = Regex.Match(input, @"\(.*\)");
            var matching_ast = Regex.Match(input, @"\*.*\*");
            int paren_open = matching_parens.Index;
            int paren_close = matching_parens.Index + matching_parens.Length;
            int ast_open = matching_ast.Index;
            int ast_close = matching_ast.Index + matching_ast.Length;
            foreach ((Regex pattern, string replacement, int chance) in patterns)
            {
                foreach (Match match in pattern.Matches(input))
                {
                    // These checks are to determine whether the match is "between" the open and close of () or **.
                    bool is_ooc = paren_open < match.Index && match.Index + match.Length <= paren_close;
                    bool is_emote = ast_open < match.Index && match.Index + match.Length <= ast_close;

                    // messy comparisons to check that the number is a) not between parens, b) not between asterisks, c) has met RNGesus sufficiently to be applied.
                    if (!is_ooc && !is_emote && (chance == 100 || rand.Next(100) < chance))
                        output = output.Replace(match.Value, match.Result(replacement));
                }
            }
            string final = output.ToString();
            foreach ((var pattern, var replacement) in profile.GlobalPatterns)
            {
                Plugin.Log.Debug($"--- Executing {pattern} replace with {replacement}");
                final = Regex.Replace(final, pattern, replacement);
            }
            return final;
        }
    }
}
