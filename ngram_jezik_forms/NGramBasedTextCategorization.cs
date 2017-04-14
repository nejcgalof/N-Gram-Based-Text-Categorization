using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;

namespace ngram_jezik_forms
{
    class NGramBasedTextCategorization
    {
        FileInfo[] Files;
        
        public string guess_language(string raw_text)
        {
            List<List<KeyValuePair<string, int>>> languages_statistics = load_ngram_statistics(); //iz vsake datoteke preberem statistiko
            List<KeyValuePair<object, object>> language_ngram_statistics = calculate_ngram_occurrences(raw_text).ToList(); //generiram statistiko za opazovan text

            //za vsako datoteko statistike jezika učne mnozice izracunam razdaljo z jezikom, katerega primerjam
            int i = 0;
            List<KeyValuePair<string, int>> results = new List<KeyValuePair<string, int>>();
            foreach (var ngrams_statistics in languages_statistics)
            {
                var distance = compare_ngram_frequency_profiles(ngrams_statistics, language_ngram_statistics); //primerjamo
                results.Add(new KeyValuePair<string, int>(Files[i].Name, distance));
                i++;
            }
            var nearest_language = results.OrderBy(x => x.Value).First().Key; //minimum je rešitev
            return nearest_language;
        }

        private int compare_ngram_frequency_profiles(List<KeyValuePair<string, int>> category_profile, List<KeyValuePair<object, object>> document_profile)
        {
            int document_distance = 0;
            List<string> category_ngrams_sorted = (from kvp in category_profile select kvp.Key).Distinct().ToList();
            List<string> document_ngrams_sorted = (from kvp in document_profile select kvp.Key.ToString()).Distinct().ToList();
            int maximum_out_of_place_value = 1000;
            foreach (var ngram in document_ngrams_sorted)
            {
                var document_index = document_ngrams_sorted.FindIndex(a => a == ngram);
                var category_profile_index = category_ngrams_sorted.FindIndex(a => a == ngram);
                if (category_profile_index == -1)
                {
                    category_profile_index = maximum_out_of_place_value; //ne najde, damo 1000
                }
                var distance = Math.Abs(category_profile_index - document_index);
                document_distance += distance;
            }
            return document_distance;
        }

        public List<List<KeyValuePair<string, int>>> load_ngram_statistics() //iz vsake datoteke preberem statistiko
        {
            DirectoryInfo d = new DirectoryInfo("languages");
            Files = d.GetFiles("*.txt"); //dobim vsa txt datoteke v mapi languages
            List<List<KeyValuePair<string, int>>> list_ngrams_statistics = new List<List<KeyValuePair<string, int>>>();
            foreach (FileInfo file in Files)
            {
                List<KeyValuePair<string, int>> ngrams_statistics = new List<KeyValuePair<string, int>>();
                foreach (string line in File.ReadLines(d.Name + "\\" + file.Name))
                {
                    string[] split = line.Split(',');
                    KeyValuePair<string, int> a = new KeyValuePair<string, int>(split[0], Convert.ToInt32(split[1]));
                    ngrams_statistics.Add(a);
                }
                list_ngrams_statistics.Add(ngrams_statistics);
            }
            return list_ngrams_statistics;
        }

        public void generate_ngram_frequency_profile_from_file(string file_path, string output_filename) //ucna mnozica
        {
            string raw_text = File.ReadAllText(file_path);
            generate_ngram_frequency_profile_from_raw_text(raw_text, output_filename);
        }

        public void generate_ngram_frequency_profile_from_raw_text(string raw_text, string filename) //ucna mnozica
        {
            IEnumerable<KeyValuePair<object, object>> sortedElements = calculate_ngram_occurrences(raw_text);
            if (!Directory.Exists("languages"))
            {
                Directory.CreateDirectory("languages");
            }
            File.WriteAllLines("languages\\" + filename, sortedElements.Select(x => x.Key + "," + x.Value));
        }

        private IEnumerable<KeyValuePair<object, object>> calculate_ngram_occurrences(string raw_text)
        {
            //pripravim besede / tokene
            raw_text = raw_text.ToLower();
            Regex rgx = new Regex("[^a-zA-ZčšžČŠŽäöüßÜÖÄ ]");
            raw_text = rgx.Replace(raw_text, "");
            string[] tokens = raw_text.Split(' '); //tokenize
            tokens = tokens.Where(x => !string.IsNullOrEmpty(x)).ToArray(); //prazne odstranim

            List<string> ngrams_list = generate_ngrams(tokens); //generiram ngrame
            Hashtable ngrams_statistics = count_ngrams_and_hash_them(ngrams_list); //kreiram hashtable s frekvencami
            //vzamem prvih 300 ngramov po frekvenci padajoče
            var dict = ngrams_statistics.Cast<DictionaryEntry>().ToDictionary(d => d.Key, d => d.Value);
            var sortedElements = dict.OrderByDescending(kvp => kvp.Value).Take(300);
            return sortedElements;
        }

        private List<string> generate_ngrams(string[] tokens)
        {
            List<string> generated_ngrams = new List<string>();
            foreach (string token in tokens)
            {
                for (int x = 1; x < 6; x++) //generiranje N gramov; N=1 do 5
                {
                    IEnumerable<string> ba = makeNgrams(token, x);
                    foreach (string b in ba)
                    {
                        generated_ngrams.Add(b);
                    }
                }
            }
            return generated_ngrams;
        }

        public IEnumerable<string> makeNgrams(string text, int numGram)
        {
            if (numGram != 1)
            {
                text = text.PadLeft(text.Length + 1, ' ');
                text = text.PadRight(text.Length + numGram - 1, ' ');
            }
            List<string> ngram = new List<string>();
            for (int pos = 0; pos <= text.Length - numGram; pos++)
            {
                ngram.Add(text.Substring(pos, numGram));
            }
            return ngram;
        }

        private Hashtable count_ngrams_and_hash_them(List<string> ngrams_list)
        {
            Hashtable ngrams_statistics = new Hashtable();
            foreach (string ngram in ngrams_list)
            {
                if (!ngrams_statistics.ContainsKey(ngram)) // ne obstaja
                {
                    ngrams_statistics.Add(ngram, 1);
                }
                else
                {
                    ngrams_statistics[ngram] = (int)ngrams_statistics[ngram] + 1;
                }
            }
            return ngrams_statistics;
        }
    }
}
