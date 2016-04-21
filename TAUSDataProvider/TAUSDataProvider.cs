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
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Security;
    using System.Xml.Linq;
    using Microsoft.Multilingual.Translation;
    using l10n = Resources.TAUSResources;

    public class TAUSDataProvider : ITranslationProvider
    {
        private readonly CredentialCache credCache = new CredentialCache();
        private readonly List<CultureInfo> supportedLanguages;
        private readonly Stream logoTranslateStandard = null;
        private readonly Stream logoSuggestStandard = null;

        private CultureInfo SourceCulture { get; set; }
        private CultureInfo TargetCulture { get; set; }

        public TAUSDataProvider(string configFile)
        {
            // Load the providers images
            var providerAssembly = Assembly.GetExecutingAssembly();
            logoTranslateStandard = providerAssembly.GetManifestResourceStream("TAUSDataProvider.Images.TAUSTranslateLogo.png");
            logoSuggestStandard = providerAssembly.GetManifestResourceStream("TAUSDataProvider.Images.TAUSSuggestLogo.png");

            // Load the configuration that contains the Credential Manager entry to use for the TAUS DATA APIs.
            var doc = XDocument.Load(configFile);
            var userElement = doc.Element("Account").Element("User");
            var url = userElement.Attribute("Url").Value;
            var type = userElement.Attribute("Type").Value;

            // Read the User name and password from the Windows Credential Manager
            string user = null;
            SecureString pass = null;
            var result = CredentialManagerHelper.ReadCredentials(url, CRED_TYPE.GENERIC, 0, out user, out pass);
            if (result == false)
                throw new InvalidProviderConfigurationException(l10n.ProviderCredentialError);

            var tausCreds = new NetworkCredential(user, pass);
            this.credCache.Add(new Uri(url), type, tausCreds);

            // Get the supported Languages List - Consider caching the previous list for better performance
            supportedLanguages = TAUSDataAccess.GetLanguages(this.credCache);
        }

        #region public methods

        /// <summary>
        /// Initializes the provider for a set language pair
        /// </summary>
        /// <param name="source">CultureInfo for source language</param>
        /// <param name="target">CultureInfo for target language</param>
        /// <param name="initializationData">Class containing the project's Name and Version</param>
        public void Initialize(CultureInfo source, CultureInfo target, ProjectInfo initializationData)
        {
            // validate parameters
            if (source == null)
                throw new ArgumentNullException("source");
            if (target == null)
                throw new ArgumentNullException("target");

            SourceCulture = source;
            TargetCulture = target;
        }

        /// <summary>
        /// Get all Supported Language pairs from the service
        /// </summary>
        /// <returns>list of supported CultureInfo</returns>
        public CultureInfo[] GetTargets(CultureInfo source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            // Is the source in the language list
            if (supportedLanguages.Find(p => p.Equals(source)) == null)
            {
                return new CultureInfo[0]; // Culture is not supported, so return an empty array
            }
            else
            {
                return supportedLanguages.ToArray();
            }
        }

        /// <summary>
        /// Checks provider supported language pairs.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool IsSupported(CultureInfo source, CultureInfo target)
        {
            // Validate parameters
            if (source == null)
                throw new ArgumentNullException("source");
            if (target == null)
                throw new ArgumentNullException("target");

            if (supportedLanguages.Find(p => p.Equals(source)) == null)
                return false;

            if (supportedLanguages.Find(p => p.Equals(target)) == null)
                return false;

            return true;
        }

        /// <summary>
        /// Attempts to translates a resource
        /// </summary>
        /// <param name="request">TranslationRequest instance</param>
        /// <returns><see cfer='TranslationResult'/></returns>
        public TranslationResult Translate(TranslationRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.Source) == true)
                throw new ArgumentException("TranslationRequest.Source");
            if (SourceCulture == null)
                throw new InvalidProviderConfigurationException("SourceCulture");
            if (TargetCulture == null)
                throw new InvalidProviderConfigurationException("TargetCulture");
            if (supportedLanguages == null)
                throw new InvalidProviderConfigurationException("supported languages has not been initialized");

            // Set default
            var result = TAUSDataAccess.GetSegmentTranslation(SourceCulture, TargetCulture, request.Source);
            result.RequestId = request.RequestId;
            return result;
        }

        /// <summary>
        /// Attempts to find translation suggestions based the specified options in SuggestionRequest object.
        /// </summary>
        /// <param name="request">Suggestion Parameters</param>
        /// <returns>Array of <see cfer='SuggestionResult'/></returns>
        public SuggestionResult[] Suggest(SuggestionRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.Source) == true)
                throw new ArgumentException("SuggestionRequest.Source");
            if (SourceCulture == null)
                throw new InvalidProviderConfigurationException("SourceCulture");
            if (TargetCulture == null)
                throw new InvalidProviderConfigurationException("TargetCulture");
            if (supportedLanguages == null)
                throw new InvalidProviderConfigurationException("support languages has not been initialized");

            return TAUSDataAccess.GetSegmentSuggestions(SourceCulture, TargetCulture, request.Source, request.ProviderMaxResults);
        }

        /// <summary>
        /// Get provider logo from provider.
        /// </summary>
        /// <returns>Image in Bitmap format</returns>
        public Stream GetProviderLogo(ProviderLogoStyle style)
        {
            switch (style)
            {
                case ProviderLogoStyle.SuggestStandard:
                    return logoSuggestStandard;

                case ProviderLogoStyle.TranslateStandard:
                    return logoTranslateStandard;

                default:
                    throw new NotSupportedException(style.ToString());
            }
        }

        /// <summary>
        /// Provider cleanup, as needed
        /// </summary>
        public void Dispose()
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Localized Display Name
        /// </summary>
        public string DisplayName
        {
            get { return l10n.ProviderDisplayName; }
        }

        /// <summary>
        /// Localized Description
        /// </summary>
        public string Description
        {
            get { return l10n.ProviderDescription; }
        }
        #endregion
    }
}
