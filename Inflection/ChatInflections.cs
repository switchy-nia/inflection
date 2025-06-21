using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Serilog;

namespace Inflection
{
    public class Inflections
    {
        const string SENTENCE_REGEX = @"([^.]*[^.]*[\.\?\!])([^.]*[^.]*)$";
        const string WORD_REGEX = @"^(\W*)([\w\W\-\']*?)(\W*)$";
        // This emoji should support the following basic text emojis
        const string EMOJI_REGEX = @"^[:;cDxX><\-\,)CP][:;><\-3)CWwdpP]*[:;cDxX><\-\,3)CWwdpP]$";

        Configuration configuration;
        Regex sentence_regex;
        Regex word_regex;
        Regex emoji_regex;
        // Regex compliance_regex;

        public Inflections(Configuration config)
        {
            configuration = config;
            sentence_regex = new Regex(SENTENCE_REGEX);
            word_regex = new Regex(WORD_REGEX);
            emoji_regex = new Regex(EMOJI_REGEX);
            // compliance_regex = new Regex(configuration.CurrentProfile.CompelledSpeech);
        }
        public bool SetToInflect()
        {
            return
                configuration.ActiveProfile.CompelledSpeechEnabled ||
                configuration.ActiveProfile.TicksEnabled ||
                configuration.ActiveProfile.PronounCorrectionEnabled ||
                configuration.ActiveProfile.StutterEnabled ||
                configuration.ActiveProfile.SentenceStartEnabled ||
                configuration.ActiveProfile.SentenceEndingEnabled;
        }
        public string Speak(String input)
        {
            Log.Debug($"Speak {input} - Start");

            StringBuilder output = new StringBuilder();
            Random rand = new Random();

            List<string> words = input.Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
            int ticks = (int)Math.Ceiling((double)words.Count * configuration.ActiveProfile.TickMaxPortionOfSpeech);
            bool to_replace = true;
            Log.Debug($"Looping over words with {words.Count}");
            for (int i = 0; i < words.Count; i++)
            {
                string full_word = words[i];
                if (isEmoji(full_word))
                {
                    Log.Debug($"{full_word} is an emoji, skipping");
                    output.Append(full_word);
                    output.Append(' ');
                    continue;
                }

                (string prefix, string word, string postfix) = handlePunctuation(full_word);
                string processed_postfix = "";
                if (prefix != "")
                {
                    if (prefix.Contains('*'))
                        to_replace = false;
                    output.Append(prefix);
                }

                if (to_replace)
                {
                    output.Append(ProcessWord(rand, word, i, words.Count, ref ticks, ref processed_postfix));
                }
                else
                {
                    output.Append(word);
                }

                if (postfix != "")
                {
                    if (postfix.Contains('*'))
                        to_replace = true;
                    output.Append(postfix);
                }
                else if (processed_postfix != "")
                {
                    output.Append(processed_postfix);
                }

                output.Append(' ');
            }

            var final = output.ToString();
            if (configuration.ActiveProfile.SentenceStartEnabled)
            {
                final = SentenceStart(rand, final);
            }

            if (configuration.ActiveProfile.SentenceEndingEnabled)
            {
                final = SentenceEnding(rand, final);
            }
            return final;
        }

        private bool isEmoji(string full_word)
        {
            return emoji_regex.IsMatch(full_word);
        }

        /// <summary>
        /// Handles the punctuation of individual word so that things like *word, are handled properly.
        /// </summary>
        /// <param name="full_word"></param>
        /// <returns>tuple that indicates a prefix punctuation, the word content, the postfix punctuation</returns>
        private (string, string, string) handlePunctuation(string full_word)
        {
            var captures = word_regex.Match(full_word).Groups;
            var prefix = captures[1].ToString();
            var word = captures[2].ToString();
            var postfix = captures[3].ToString();
            switch (configuration.ActiveProfile.VoiceType)
            {

                case VoiceVolume.Small:
                    prefix = prefix.Replace(".", "...");
                    prefix = prefix.Replace("!", "...");
                    prefix = prefix.Replace("?", "...");
                    postfix = postfix.Replace(".", "...");
                    postfix = postfix.Replace("!", "..!");
                    postfix = prefix.Replace("?", "..?");
                    break;
                case VoiceVolume.Big:
                    prefix = prefix.Replace(".", "!");
                    prefix = prefix.Replace("!", "!!!");
                    prefix = prefix.Replace("?", "!?");
                    postfix = postfix.Replace(".", "!");
                    postfix = postfix.Replace("!", "!!");
                    postfix = prefix.Replace("?", "!?");
                    break;
            }
            return (prefix, word, postfix);
        }

        // Takes the word and processes it based on the specific words
        private string ProcessWord(Random rand, string word, int word_index, int total_words, ref int ticks, ref string postfix_punctuation)
        {
            // By default it will be empty
            postfix_punctuation = "";
            switch (configuration.ActiveProfile.VoiceType)
            {
                case VoiceVolume.Small:
                    word = word.ToLower();
                    break;
                case VoiceVolume.Big:
                    word = word.ToUpper();
                    break;
            }


            // Forced speech trumps all other speech and returns immediately, no further processing required.
            if (configuration.ActiveProfile.CompelledSpeechEnabled)
            {
                Log.Debug($"Process {word} using forced speech and early returning");
                int index = rand.Next(configuration.ActiveProfile.CompelledSpeechWords.Count);
                return configuration.ActiveProfile.CompelledSpeechWords.ElementAt(index);
            }

            // If this is found in a pronouns list, change it.
            if (configuration.ActiveProfile.PronounCorrectionEnabled)
            {
                var key = word.ToLower();
                if (configuration.ActiveProfile.PronounsReplacements.ContainsKey(key))
                {
                    Log.Debug($"Process {word} using forced pronouns");
                    word = configuration.ActiveProfile.PronounsReplacements[key];
                }
            }

            // Roll for stuttering. TODO: Make configurable.
            if (configuration.ActiveProfile.StutterEnabled && rand.Next(100) < configuration.ActiveProfile.StutterChance)
            {
                Log.Debug($"Process {word} and making it stutter");
                string stutter = "";
                int max_stutters = rand.Next(configuration.ActiveProfile.MaxStutterSeverity);
                for (int i = 0; i < 1 + max_stutters; i++)
                {
                    stutter += word.First() + "-";
                }
                // Randomize it with a slightly more drammatic stutter
                rand.Next(configuration.ActiveProfile.MaxStutterSeverity);
                word = stutter + word;
            }

            // Finally if there is an utterance required, add it.
            // The number of ticks here is to respect the maximum portion of a sentence
            // (to prevent RNG from making people have a spasm of utterances unless they want to.)
            if (configuration.ActiveProfile.TicksEnabled && word_index <= total_words && ticks > 0)
            {
                Log.Debug($"Process {word} verbal ticks");

                // Simple bias to try to ensure that you are twice as likely to have something occur near the end as the beginning
                // 1 / 10 = much lower chance to roll sufficiently
                // 5 / 10 = 1f (no mod to roll)
                // 10 / 10 = almost double the roll bias.
                // Note: For simplicity, the roll calculation is measuring less than for the chance .
                var bias = total_words / (1 + word_index);
                var roll = rand.NextDouble() * bias;

                if (configuration.ActiveProfile.Ticks.Count == 0)
                {
                    Log.Debug($"No verbal ticks found, this should not be possible to set, so there is an error in your configuration.CurrentProfile.");
                }
                else if (roll < configuration.ActiveProfile.TickChance)
                {
                    ticks -= 1;
                    int index = rand.Next(configuration.ActiveProfile.Ticks.Count);
                    string tick = configuration.ActiveProfile.Ticks.ElementAt(index);
                    word = word + ", " + tick;
                    postfix_punctuation = ",";
                }
            }

            return word;
        }

        private string Pronouns(string input)
        {
            StringBuilder output = new StringBuilder();

            foreach (string full_word in input.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                if (isEmoji(full_word))
                {
                    output.Append(full_word);
                    continue;
                }
                (string pre, string word, string post) = handlePunctuation(full_word);
                output.Append(pre);

                if (configuration.ActiveProfile.PronounsReplacements.ContainsKey(word))
                {
                    output.Append(configuration.ActiveProfile.PronounsReplacements[word]);
                }
                else
                {
                    output.Append(word);
                }
                output.Append(post);
                output.Append(" ");
            }

            return output.ToString().TrimEnd();
        }

        private string SentenceStart(Random rand, string input)
        {
            if (configuration.ActiveProfile.SentenceStartEnabled)
            {
                int index = rand.Next(configuration.ActiveProfile.SentenceStarts.Count);
                return configuration.ActiveProfile.SentenceStarts.ElementAt(index) + input;
            }
            else
            {
                return input;
            }
        }

        private string SentenceEnding(Random rand, string input)
        {
            if (configuration.ActiveProfile.SentenceEndingEnabled)
            {
                int index = rand.Next(configuration.ActiveProfile.SentenceEndings.Count);
                return input + configuration.ActiveProfile.SentenceEndings.ElementAt(index);
            }
            else
            {
                return input;
            }
        }

        private string bimboify(string input)
        {
            // To bimboify speech as per: https://github.com/Gardamuse/mispell/blob/master/src/bimbofy.js
            // it may be a bit of a longer term project, so this is strictly a "TODO" function.
            return input;
        }
    }
}
