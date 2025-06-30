using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
// using Serilog;
namespace Inflection
{
    class Cooldown
    {
        int count = 0;
        int cooldown = -1;
        bool CanExecute { get { return count >= cooldown; } }
        Cooldown(int cooldown)
        {
            this.cooldown = cooldown;
        }
        public void Tick()
        {
            count++;
        }
        public void Reset()
        {
            count = 0;
        }
    }

    public class Inflections
    {
        const string PUNCTUATION_PATTERN = @"([\.\?\!\[\]\{\}\,\(\)]|INFOPENEMOTE|INFCLOSEEMOTE)";
        const string SENTENCE_REGEX = @"([^.]*[^.]*[\.\?\!])([^.]*[^.]*)$";
        const string WORD_REGEX = @"^(\W*)([\w\W\-\']*?)(\W*)$";
        // This emoji should support the following basic text emojis
        const string EMOJI_REGEX = @"^[:;cDxX><\-\,)CP][:;><\-3)CWwdpP]*[:;cDxX><\-\,3)CWwdpP]$";
        Profile profile;
        Regex punctuationRegex;
        Regex sentence_regex;
        Regex word_regex;
        Regex emoji_regex;

        public Inflections(Profile new_profile)
        {
            profile = new_profile;
            punctuationRegex = new Regex(PUNCTUATION_PATTERN, RegexOptions.Compiled);
            sentence_regex = new Regex(SENTENCE_REGEX, RegexOptions.Compiled);
            word_regex = new Regex(WORD_REGEX, RegexOptions.Compiled);
            emoji_regex = new Regex(EMOJI_REGEX, RegexOptions.Compiled);
        }

        public bool SetToInflect()
        {
            return
                profile.CompelledSpeechEnabled ||
                profile.TicksEnabled ||
                profile.PronounCorrectionEnabled ||
                profile.StutterEnabled ||
                profile.SentenceStartEnabled ||
                profile.SentenceEndingEnabled;
        }
        public string Speak(String input)
        {
            //Log.Debug($"Speak {input} - Start");

            StringBuilder output = new StringBuilder();
            Random rand = new Random();
            // First tokenize it.
            // Simple method of ensuring all punctuation and words have a space between them, then splitting by space.
            // First we need to handle the RP tag '*', this is mostly for formatting purposes later (only one set is supported for now).
            var tokens = Regex.Replace(input, @"\*(.*)\*", "INFOPENEMOTE $1 INFCLOSEEMOTE");
            tokens = tokens.Replace("*", "");
            tokens = punctuationRegex.Replace(tokens, @" $1 ");
            List<string> words = tokens.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

            int ticks = (int)Math.Ceiling((double)words.Count * profile.TickMaxPortionOfSpeech);
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
                    output.Append(ProcessWord(rand, word, i, words.Count, ref ticks));
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
            // undo the tokenization. 
            // And restore the message formatting now that we're done messing with it.
            final = Regex.Replace(final, @"([\(\[\{])\s+(\w)", "$1$2");
            final = Regex.Replace(final, @"\s+([\)\]\}\,\.\!\?])", "$1");

            final = Regex.Replace(final, @"([\w\.\!\?])\s+([\)\]\}\,\.\!\?])", "$1$2");
            final = Regex.Replace(final, @"INFOPENEMOTE\s+", "*");
            final = Regex.Replace(final, @"\s+INFCLOSEEMOTE", "*");
            // final = Regex.Replace(final, @"([\(\[\{\*])\s+(\w)", "$1$2");
            // final = Regex.Replace(final, @"([\w\.\!\?])\s+([\)\]\}\,\.\!\?\*])", "$1$2");
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
        private string ProcessWord(Random rand, string word, int word_index, int total_words, ref int ticks)
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
            if (profile.PronounCorrectionEnabled)
            {
                var key = word.ToLower();
                if (profile.PronounsReplacements.ContainsKey(key))
                {
                    //Log.Debug($"Process {word} using forced pronouns");
                    word = profile.PronounsReplacements[key];
                }
            }

            // Roll for stuttering. TODO: Make configurable.
            if (profile.StutterEnabled && rand.Next(100) < profile.StutterChance)
            {
                //Log.Debug($"Process {word} and making it stutter");
                string stutter = "";
                // Randomize it with a slightly more drammatic stutter
                int max_stutters = rand.Next(profile.MaxStutterSeverity);
                for (int i = 0; i < 1 + max_stutters; i++)
                {
                    stutter += word.First() + "-";
                }
                word = stutter + word;
            }

            // Finally if there is an utterance required, add it.
            // The number of ticks here is to respect the maximum portion of a sentence
            // (to prevent RNG from making people have a spasm of utterances unless they want to.)
            if (profile.TicksEnabled && word_index <= total_words && ticks > 0)
            {
                //Log.Debug($"Process {word} verbal ticks");

                // Simple bias to try to ensure that you are twice as likely to have something occur near the end as the beginning
                // 1 / 10 = much lower chance to roll sufficiently
                // 5 / 10 = 1f (no mod to roll)
                // 10 / 10 = almost double the roll bias.
                // Note: For simplicity, the roll calculation is measuring less than for the chance .
                var bias = total_words / (1 + word_index);
                var roll = rand.NextDouble() * bias;

                if (profile.Ticks.Count == 0)
                {
                    //Log.Debug($"No verbal ticks found, this should not be possible to set, so there is an error in your configuration.CurrentProfile.");
                }
                else if (roll < profile.TickChance)
                {
                    ticks -= 1;
                    int index = rand.Next(profile.Ticks.Count);
                    string tick = profile.Ticks.ElementAt(index);
                    word = word + " " + tick;
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
            return input + " " + profile.SentenceEndings.ElementAt(index);
        }

        private string bimboify(string input)
        {
            // To bimboify speech as per: https://github.com/Gardamuse/mispell/blob/master/src/bimbofy.js
            // it may be a bit of a longer term project, so this is strictly a "TODO" function.
            return input;
        }
    }
}
