using System;
using System.Collections.Generic;
using LookupWord_Api.Services.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using LookupWord_Api.Model;
using System.Text.RegularExpressions;
using static Azure.Core.HttpHeader;
using LookupWord_Api.ResponseVO;

namespace LookupWord_Api.Services
{
    public class WiktionaryService : IDictionaryService
    {
        public async Task<Object> getDefinition(string word)
        {
            string rawDefinition;
            try
            {
                // get the raw definition from wiktionary...
                rawDefinition = await getRawDefinitionFromWiktionary(word);
            }
            catch (Exception ex)
            {
                return new ErrorResponse
                {
                    error = "Not Found on Wiktionary"
                };
            }

            // parse it...
            Object parsedResponse = parseWiktionaryDefinition(rawDefinition);


            // return it
            return parsedResponse;
        }

        public async Task<string> getRawDefinitionFromWiktionary(string word)
        {
            using (HttpClient client = new())
            {
                var uriEncodedWord = HttpUtility.UrlEncode(word);
                var rawDefinition = 
                    await client.GetStringAsync("https://en.wiktionary.org/w/index.php?action=raw&title=" + uriEncodedWord);

                return rawDefinition;
            }
        }

        public Object parseWiktionaryDefinition(string rawDefinition)
        {
            string justTheGermanDefinition = getJustTheGermanDefinition(rawDefinition);
            if (!justTheGermanDefinition.Equals(""))
            {
                WiktionaryDefinition germanDefinitionParsed = parseGermanDefinition(justTheGermanDefinition);
                return germanDefinitionParsed;
            }

            return new ErrorResponse
            {
                error = "Couldn't retrieve definition. German definition doesn't exist."
            };
        }

        public string getJustTheGermanDefinition(string rawDefinition)
        {
            Regex germanRegex = new("==German==", RegexOptions.Multiline);
            if (germanRegex.IsMatch(rawDefinition))
            {
                Regex languageRegex = new("^==[a-zA-Z ]+==$", RegexOptions.Multiline);
                string[] languageSections = languageRegex.Split(rawDefinition);
                MatchCollection mclanguages = languageRegex.Matches(rawDefinition);
                string[] languages = mclanguages.Select(x => x.Groups[0].Value).ToArray();

                int germanIndex = Array.IndexOf(languages, "==German==");
                string germanSection = languageSections[germanIndex + 1];

                return germanSection.Trim();
            } 
            else
            {
                return "";
            }
        }

        public WiktionaryDefinition parseGermanDefinition(string rawDefinition)
        {
            var result = new WiktionaryDefinition
            {
                definitions = new List<Definition>()
            };

            Regex subSectionRegex = new("^====?[a-zA-Z ]+====?$", RegexOptions.Multiline);
            string[] subSections = subSectionRegex.Split(rawDefinition).Skip(1).ToArray();

            MatchCollection mcsubSectionHeaders = subSectionRegex.Matches(rawDefinition);
            string[] subSectionHeaders = mcsubSectionHeaders.Select(x => x.Groups[0].Value.Replace("=", "")).ToArray();

            // ladies and gentlemen, we're using Tuples
            var headersAndContents = subSections.Zip(subSectionHeaders, (first, second) => (first, second));

            string[] partsOfSpeech = {"Conjunction", "Noun", "Verb", "Adjective",
                    "Preposition", "Adverb", "Interjection", "Pronoun" };

            foreach (var headerAndContentTuple in headersAndContents)
            {
                string subSection = headerAndContentTuple.first;
                string subSectionHeader = headerAndContentTuple.second;

                int partOfSpeechIndex = Array.IndexOf(partsOfSpeech, subSectionHeader);
                if (partOfSpeechIndex == -1)
                    continue;

                Regex meaningRegex = new("^# (.*)$", RegexOptions.Multiline);
                MatchCollection mcDefinitions = meaningRegex.Matches(subSection);
                string[] definitions = mcDefinitions.Select(x => x.Groups[1].Value).ToArray();

                Definition d = new Definition
                {
                    partOfSpeech = partsOfSpeech[partOfSpeechIndex].Replace("=", ""),
                    meanings = definitions.Select(x =>
                    {
                        string retVal = Regex.Replace(x, "[\\[\\]]", "");
                        retVal = Regex.Replace(retVal, "\\{\\{m\\|mul\\|([a-zA-Z ]+?)\\}\\}", "$1");
                        retVal = Regex.Replace(retVal, "\\{\\{ngd\\|([^}]+?)\\}\\}", "$1");
                        retVal = Regex.Replace(retVal, "\\{\\{gloss\\|([^}]+?)\\}\\}", "($1)");
                        retVal = Regex.Replace(retVal, "\\{\\{non-gloss definition\\|([^}]+?)\\}\\}", "$1");
                        retVal = Regex.Replace(retVal, "\\{\\{\\+preo\\|de\\|(\\w+)\\|(accusative|dative|genitive)\\}\\}", "[+ $1 ($2)]");
                        retVal = Regex.Replace(retVal, "#\\w+\\|\\w+", "");
                        retVal = retVal.Trim();

                        Regex lbDeRegex = new("\\{\\{lb\\|de\\|([a-zA-Z _|+]+?)\\}\\}");
                        MatchCollection mcLbDeTemplates = lbDeRegex.Matches(retVal);
                        (string, string)[] lbDeTemplates =
                            mcLbDeTemplates.Select(x => (x.Groups[0].Value, x.Groups[1].Value)).ToArray();

                        foreach (var t in lbDeTemplates) {
                            string lbDe = t.Item2;
                            string[] notes = lbDe.Split("\\|");
                            string commafiedNotes = String.Join(", ", notes);
                            string replacementText = "(" + commafiedNotes + ")";
                            retVal = retVal.Replace(t.Item1, replacementText);
                        }
                        return retVal;
                    }).ToList()
                };

                result.definitions.Add(d);
            }
            return result;

        }
    }
}
