using System.Collections.Generic;
using System.Linq;

namespace ConstructionLine.CodingChallenge
{
    public class SearchEngine
    {
        private readonly List<Shirt> _shirts;
        private readonly Dictionary<string, List<Shirt>> _shirtsDictionary;

        public SearchEngine(List<Shirt> shirts)
        {
            _shirts = shirts;

            if (_shirts != null)
            {
                _shirtsDictionary = _shirts
                                  .GroupBy(r => new { colorName = r.Color.Name, sizeName = r.Size.Name })
                                  .Select(g => new { shirtKey = DictionaryKey(g.Key.colorName, g.Key.sizeName), shirts = g.ToList() })
                                  .ToDictionary(t => t.shirtKey, t => t.shirts);
            }
        }


        public SearchResults Search(SearchOptions options)
        {
            if (_shirts == null)
                return new SearchResults { };

            List<Shirt> shirtSearchResults = _shirts;

            if (options != null)
            {
                shirtSearchResults = new List<Shirt>();

                if (options.Colors.Any() && options.Sizes.Any())
                {
                    // Create all possilbe keys
                    var searchOptionkeys = options.Colors.Zip(options.Sizes, (c, s) => new { shirtKey = DictionaryKey(c.Name, s.Name) })
                        .Select(x => x.shirtKey).ToList();

                    var keys = _shirtsDictionary.Where(x => searchOptionkeys.Contains(x.Key)).Select(x => x.Key).ToList();

                    PopulateSearchResult(shirtSearchResults, keys);
                }
                else if (options.Colors.Any())
                {
                    // Create all possilbe part of the keys
                    var keyParts = options.Colors.Select(x => x.Name).ToList();
                    foreach (var kayPart in keyParts)
                    {
                        var keys = _shirtsDictionary.Where(x => x.Key.StartsWith(kayPart)).Select(x => x.Key).ToList();

                        PopulateSearchResult(shirtSearchResults, keys);
                    }
                }
                else if (options.Sizes.Any())
                {
                    // Create all possilbe part of the keys
                    var keyParts = options.Sizes.Select(x => x.Name).ToList();
                    foreach (var keyPart in keyParts)
                    {
                        var keys = _shirtsDictionary.Where(x => x.Key.EndsWith(keyPart)).Select(x => x.Key).ToList();

                        PopulateSearchResult(shirtSearchResults, keys);
                    }
                }
            }

            return new SearchResults
            {
                Shirts = shirtSearchResults,
                SizeCounts = (from size in Size.All
                              join sizeFound in shirtSearchResults on size equals sizeFound.Size into sizeCount
                              select new SizeCount { Size = size, Count = sizeCount.Count() }).ToList(),
                ColorCounts = (from color in Color.All
                               join colorFound in shirtSearchResults on color equals colorFound.Color into colorCount
                               select new ColorCount { Color = color, Count = colorCount.Count() }).ToList()
            };
        }

        private void PopulateSearchResult(List<Shirt> shirtSearchResults, List<string> keys)
        {
            foreach (var key in keys)
            {
                shirtSearchResults.AddRange(_shirtsDictionary[key]);
            }
        }

        private string DictionaryKey(string colorName, string sizeName)
        {
            return $"{colorName}-{sizeName}";
        }
    }
}