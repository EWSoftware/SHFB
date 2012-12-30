// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/26/2012 - EFW - Moved the classes into the Targets namespace

using System;

namespace Microsoft.Ddue.Tools.Targets
{
    public class ArrayTypeReference : TypeReference
    {
        private int rank;

        private TypeReference elementType;

        public int Rank
        {
            get
            {
                return (rank);
            }
        }

        public TypeReference ElementType
        {
            get
            {
                return (elementType);
            }
        }

        internal ArrayTypeReference(TypeReference elementType, int rank)
        {
            if(elementType == null)
                throw new ArgumentNullException("elementType");
            if(rank <= 0)
                throw new ArgumentOutOfRangeException("rank");
            this.elementType = elementType;
            this.rank = rank;
        }
    }
}
