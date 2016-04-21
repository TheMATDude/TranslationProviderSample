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
    using System.Collections;
    using Microsoft.Multilingual.Translation;

    /// <summary>
    /// Suggestion Confidence level comparer
    /// </summary>
    internal class SuggestionConfidenceComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            var xResult = x as SuggestionResult;
            var yResult = y as SuggestionResult;

            // Get the nulls out of the way.
            if (xResult == null && yResult == null)
                return 0;

            if (xResult == null)
                return 1;

            if (yResult == null)
                return -1;

            if (xResult.Confidence < yResult.Confidence)
                return 1;

            if (xResult.Confidence > yResult.Confidence)
                return -1;

            return 0;
        }
    }
}
