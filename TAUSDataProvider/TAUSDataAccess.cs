// The MIT License(MIT)
//
// Copyright(c) 2016  Microsoft Corporation. All Rights Reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
// associated documentation files (the "Software"), to deal in the Software without restriction, 
// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE 
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR 
// IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

namespace TAUSDataProvider
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;
    using System.Xml.Linq;

    using Microsoft.Multilingual.Translation;

    using l10n = Resources.TAUSResources;

    /// <summary>
    /// Using the TAUS terminology server as a translation provider
    /// </summary>
    /// <remarks>
    /// Since TAUS DATA is a designed as a terminology lookup service, the results will include any word that matches.
    /// </remarks>
    internal class TAUSDataAccess
    {
        /// <summary>
        /// Store the retrieve credentials for this session
        /// </summary>
        private static CredentialCache CredCache { get; set; }

        /// <summary>
        /// Get the list of support languages
        /// </summary>
        /// <param name="credCache"></param>
        /// <returns>List of support languages</returns>
        /// <remarks>
        /// The TAUS DATA API supports multi-directional lookups across all 
        /// languages so no special language pair matching required.
        /// </remarks>
        internal static List<CultureInfo> GetLanguages(CredentialCache credCache)
        {
            CredCache = credCache;

            var httpRequest = (HttpWebRequest)WebRequest.Create("https://www.tausdata.org/api/lang.xml");
            httpRequest.Credentials = CredCache;

            var languages = new List<CultureInfo>();

            // Read the contents of the response and load in the XDocument
            var docResponse = GetXmlFormatResponse(httpRequest);
            if (docResponse != null)
            {
                foreach (var langID in docResponse.Descendants("id"))
                {
                    try
                    {
                        languages.Add(new CultureInfo(langID.Value));
                    }
                    catch (CultureNotFoundException) { }  // Ignore unknown cultures
                }
            }

            return languages;
        }

        /// <summary>
        /// Queries the provider for matching translation 
        /// </summary>
        /// <param name="srcCulture">Culture of the requested string</param>
        /// <param name="trgCulture">Culture of the resulting strings</param>
        /// <param name="srcString">String to translate</param>
        /// <returns>TranslationRequest</returns>
        /// <remarks>
        /// Multiple results are returned, so this filters it down to a single result, or none if match is found.
        /// </remarks>
        internal static TranslationResult GetSegmentTranslation(CultureInfo srcCulture, CultureInfo trgCulture, string srcString)
        {
            var encodedSource = srcString.Replace(' ', '+');
            var urlString = string.Format("https://www.tausdata.org/api/segment.xml?source_lang={0}&target_lang={1}&q={2}&fuzzy=false", srcCulture.Name, trgCulture.Name, encodedSource);

            var httpRequest = (HttpWebRequest)WebRequest.Create(urlString);
            httpRequest.Credentials = CredCache;

            // Set default
            var transResult = new TranslationResult()
            {
                // RequestId = request.RequestId,
                TranslationState = new TranslationState() { State = TransState.NoMatch, SubState = "no match found" }
            };

            // Read the contents of the response and load it into an XDocument
            var docResponse = GetXmlFormatResponse(httpRequest);
            if (docResponse != null)
            {
                foreach (var trans in docResponse.Descendants("segment"))
                {
                    var source = trans.Element("source").Value;

                    // Select the best translation that mets the minimum confidence requirement.
                    var confidence = CalculateConfidence(srcString, source);
                    if (confidence > transResult.Confidence)
                    {
                        transResult.ProviderName = l10n.ProviderDisplayName;
                        transResult.Source = source;
                        transResult.Target = trans.Element("target").Value;
                        transResult.TranslationState = new TranslationState() { State = TransState.NeedsReview };
                        transResult.TranslationType = new TranslationType() { Type = TransType.TranslationMemory };
                        transResult.Properties = GetMetadata(trans);
                        transResult.Confidence = confidence;
                    }

                    // If we have 100% confidence, stop looking for a better translation.
                    if (transResult.Confidence == 100.0)
                    {
                        // Mark as translated instead of needs review
                        transResult.TranslationState.State = TransState.Translated;
                        break;
                    }
                }
            }

            return transResult;
        }
        

        /// <summary>
        /// Queries the provider for matching translation suggestions
        /// </summary>
        /// <param name="srcCulture">Culture of the requested string</param>
        /// <param name="trgCulture">Culture of the resulting strings</param>
        /// <param name="srcString">String to translate</param>
        /// <param name="maxResults">Maximum allowed results</param>
        /// <returns>Zero to maxResult of SuggestionResult[]</returns>
        /// <remarks>
        /// Multiple results are returned, so this filters it down to the maximum allows results.
        /// </remarks>
        internal static SuggestionResult[] GetSegmentSuggestions(CultureInfo srcCulture, CultureInfo trgCulture, string srcString, int maxResults)
        {
            var encodedSource = srcString.Replace(' ', '+');
            var urlString = string.Format("https://www.tausdata.org/api/segment.xml?source_lang={0}&target_lang={1}&q={2}&fuzzy=false", srcCulture.Name, trgCulture.Name, encodedSource);

            var httpRequest = (HttpWebRequest)WebRequest.Create(urlString);
            httpRequest.Credentials = CredCache;

            // Set default result
            var suggestResults = new List<SuggestionResult>();

            // Read the contents of the response and load it into an XDocument
            var docResponse = GetXmlFormatResponse(httpRequest);
            if (docResponse != null)
            {
                foreach (var trans in docResponse.Descendants("segment"))
                {
                    var source = trans.Element("source").Value;
                    var target = trans.Element("target").Value;

                    var suggestResult = new SuggestionResult()
                    {
                        Source = source, 
                        Target = target, 
                        ProviderName = l10n.ProviderDisplayName, 
                        TranslationState = new TranslationState() { State = TransState.NeedsReview }, 
                        TranslationType = new TranslationType() { Type = TransType.TranslationMemory }
                    };

                    suggestResult.Confidence = CalculateConfidence(srcString, suggestResult.Source);
                    suggestResult.Properties = GetMetadata(trans);
                    suggestResults.Add(suggestResult);
                    if (suggestResults.Count >= maxResults)
                    {
                        break;
                    }
                }
            }

            // The code assumes the result are in best to worst matching order.
            var sortedResults = suggestResults.ToArray();
            Array.Sort(sortedResults, new SuggestionConfidenceComparer());
            return sortedResults;
        }

        /// <summary>
        /// Calculates the Confidence Level from the original and referenced source values
        /// </summary>
        /// <param name="originalSource"></param>
        /// <param name="referenceSource"></param>
        /// <returns>Confidence level</returns>
        /// <remarks>
        /// This ad-hoc confidence level calculation is needed since the TAUS service 
        /// is designed as a word level match for terminology lookup as compared to a 
        /// translation service.  If this were to be used in production, using an 
        /// industry standard confidence level should be used.
        /// </remarks>
        private static double CalculateConfidence(string originalSource, string referenceSource)
        {
            // If the source strings match exactly, assume 100 confidence level
            if (originalSource.Equals(referenceSource, StringComparison.InvariantCulture) == true)
                return 100;

            // If the source strings match exactly - except for case, assume 95 confidence level
            if (originalSource.Equals(referenceSource, StringComparison.InvariantCultureIgnoreCase) == true)
                return 95;

            // Since the min default confidence level is 75 for Translate and 50 for suggest, 
            // Using a default of 50 returns all responses for suggest API, but filters for translate APIs
            return 50;  
        }

        /// <summary>
        /// Retrieve the metadata from the response
        /// </summary>
        /// <param name="transResponse">Translation response node</param>
        /// <returns>TAUSDataProviderProperties as an object</returns>
        private static object GetMetadata(XElement transResponse)
        {
            return new TAUSDataProviderProperties
                        {
                            Provider = GetMetadatItem(transResponse, "provider"),
                            Owner = GetMetadatItem(transResponse, "owner"),
                            Industry = GetMetadatItem(transResponse, "industry"),
                            ContentType =GetMetadatItem(transResponse, "content_type"),
                            Product = GetMetadatItem(transResponse, "product")
                        };
        }

        /// <summary>
        /// Get the related metadata name and value
        /// </summary>
        /// <param name="transResponse"></param>
        /// <param name="elementName"></param>
        /// <returns>Return the metadata value, or null if not found</returns>
        private static string GetMetadatItem(XElement transResponse, string elementName)
        {
            if (transResponse.Element(elementName) != null && transResponse.Element(elementName).Element("name") != null)
                return transResponse.Element(elementName).Element("name").Value;

            return null;
        }

        /// <summary>
        /// Get the XML based response
        /// </summary>
        /// <param name="httpRequest">The HTTP request</param>
        /// <returns>XDocument</returns>
        private static XDocument GetXmlFormatResponse(HttpWebRequest httpRequest)
        {
            var httpResponse = (HttpWebResponse)httpRequest.GetResponse();

            // Read the contents of the response and load it in the XDocument
            XDocument docResponse;
            using (var responseData = httpResponse.GetResponseStream())
            {
                docResponse = XDocument.Load(responseData);
            }

            httpResponse.Close();

            return docResponse;
        }
    }
}
