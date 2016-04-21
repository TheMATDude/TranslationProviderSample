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
    using System.ComponentModel;

    /// <summary>
    /// Metadata support by the TAUS DATA APIs
    /// </summary>
    public class TAUSDataProviderProperties
    {
        [Category("Metadata")]
        [LocalizedDisplayNameAttribute("PropertyProductDisplayName")]
        [LocalizedDescriptionAttribute("PropertyProductDescription")]
        public string Product { get; set; }

        [Category("Metadata")]
        [LocalizedDisplayNameAttribute("PropertyProviderDisplayName")]
        [LocalizedDescriptionAttribute("PropertyProviderDescription")]
        public string Provider { get; set; }

        [Category("Metadata")]
        [LocalizedDisplayNameAttribute("PropertyContentTypeDisplayName")]
        [LocalizedDescriptionAttribute("PropertyContentTypeDescription")]
        public string ContentType { get; set; }

        [Category("Metadata")]
        [LocalizedDisplayNameAttribute("PropertyIndustryDisplayName")]
        [LocalizedDescriptionAttribute("PropertyIndustryDescription")]
        public string Industry { get; set; }

        [Category("Metadata")]
        [LocalizedDisplayNameAttribute("PropertyOwnerDisplayName")]
        [LocalizedDescriptionAttribute("PropertyOwnerDescription")]
        public string Owner { get; set; }
    }

    /// <summary>
    /// Provides the Localized metadata's display name
    /// </summary>
    /// <remarks>
    /// See: http://msdn.microsoft.com/en-us/library/vstudio/System.ComponentModel.DisplayNameAttribute(v=vs.100).aspx
    /// </remarks>
    public sealed class LocalizedDisplayNameAttribute : DisplayNameAttribute
    {
        private readonly string resourceName;
        public LocalizedDisplayNameAttribute(string resourceName)
        {
            this.resourceName = resourceName;
        }

        public override string DisplayName
        {
            get
            {
                return Resources.TAUSResources.ResourceManager.GetString(this.resourceName);
            }
        }
    }

    /// <summary>
    /// Provides the Localized metadata's description.
    /// </summary>
    /// <remarks>
    /// See: http://msdn.microsoft.com/en-us/library/vstudio/system.componentmodel.descriptionattribute(v=vs.100).aspx
    /// </remarks>
    public sealed class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        private readonly string resourceName;
        public LocalizedDescriptionAttribute(string resourceName)
        {
            this.resourceName = resourceName;
        }

        public override string Description
        {
            get
            {
                return Resources.TAUSResources.ResourceManager.GetString(this.resourceName);
            }
        }
    }
}
