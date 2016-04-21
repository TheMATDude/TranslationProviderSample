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
    using System.Runtime.InteropServices;
    using System.Security;
    using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

    /// <summary>
    /// Helper class for access data in the Credential Manager.  
    /// </summary>
    /// <remarks>
    /// See: http://windows.microsoft.com/en-US/Windows7/What-is-Credential-Manager?woldogcb=0
    /// </remarks>
    internal class CredentialManagerHelper
    {
        [DllImport("Advapi32.dll", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CredRead(string target, CRED_TYPE type, uint reservedFlag, out IntPtr CredentialPtr);
        [DllImport("Advapi32.dll", EntryPoint = "CredFree", SetLastError = true)]
        private static extern bool CredFree([In] IntPtr cred);

        /// <summary>
        /// Read the user name and password from the Windows Credential Manager
        /// </summary>
        /// <param name="targetName">name of the credential to read</param>
        /// <param name="credType">Type of the credential to read</param>
        /// <param name="flags">Currently reserved and must be zero</param>
        /// <param name="userName">Stored user name</param>
        /// <param name="password">Store password</param>
        /// <returns></returns>
        public static bool ReadCredentials(string targetName, CRED_TYPE credType, uint flags, out string userName, out SecureString password)
        {
            userName = null;
            password = null;

            var pCredentials = IntPtr.Zero;

            // See: http://msdn.microsoft.com/en-us/library/windows/desktop/aa374804(v=vs.85).aspx
            var result = CredRead(targetName, credType, flags, out pCredentials);
            if (result == true)
            {
                var cred = (Credential)Marshal.PtrToStructure(pCredentials, typeof(Credential));

                userName = cred.UserName;
                password = new SecureString();
                foreach (var passChar in cred.CredentialBlob)
                {
                    password.AppendChar(passChar);
                }

                password.MakeReadOnly();

                // See: http://msdn.microsoft.com/en-us/library/windows/desktop/aa374796(v=vs.85).aspx
                CredFree(pCredentials);
            }

            return result;
        }
    }

    /// <summary>
    /// Derived from _CREDENTIALW in %ProgramFiles(x86)%\Windows Kits\8.0\Include\um\wincred.h
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct Credential
    {
        public uint Flags;
        public CRED_TYPE Type;
        public string TargetName;
        public string Comment;
        public FILETIME LastWritten;
        public uint CredentialBlobSize;
        public string CredentialBlob;
        public CRED_PERSIST Persist;
        public uint AttributeCount;
        public IntPtr Attributes;
        public string TargetAlias;
        public string UserName;
    }

    /// <summary>
    /// Values from CRED_TYPE_* enum in %ProgramFiles(x86)%\Windows Kits\8.0\Include\um\wincred.h
    /// </summary>
    internal enum CRED_TYPE : uint
    {
        GENERIC = 1, 
        DOMAIN_PASSWORD = 2, 
        DOMAIN_CERTIFICATE = 3, 
        DOMAIN_VISIBLE_PASSWORD = 4, 
        GENERIC_CERTIFICATE = 5, 
        DOMAIN_EXTENDED = 6, 
        MAXIMUM = 7,                    // Maximum supported cred type
        MAXIMUM_EX = MAXIMUM + 1000   // Allow new applications to run on old OSes
    }

    /// <summary>
    /// Values from CRED_PERSIST_* in %ProgramFiles(x86)%\Windows Kits\8.0\Include\um\wincred.h
    /// </summary>
    internal enum CRED_PERSIST : uint
    {
        NONE = 0, 
        SESSION = 1, 
        LOCAL_MACHINE = 2, 
        ENTERPRISE = 3
    }
}
