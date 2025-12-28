/*!
 * Lunr languages, `French` language
 * https://github.com/MihaiValentin/lunr-languages
 *
 * Copyright 2014, Mihai Valentin
 * http://www.mozilla.org/MPL/
 *
 * based on
 * Snowball JavaScript Library v0.3
 * http://code.google.com/p/urim/
 * http://snowball.tartarus.org/
 *
 * Copyright 2010, Oleg Mazko
 * http://www.mozilla.org/MPL/
 *
 * Original JavaScript source and project website: https://github.com/MihaiValentin/lunr-languages
 *
 * EFW - 12/24/2025 - Ported to C# using Copilot.  This is a minimal port that only includes the languages
 * currently supported by the help file builder.
 */

using System;
using System.Collections.Generic;

namespace SandcastleBuilder.MSBuild.Lunr.Languages;

/// <summary>
/// French language support for Lunr.
/// Provides stemming and stop word filtering for French text.
/// </summary>
public static class French
{
    /// <summary>
    /// Word characters specific to French language.
    /// </summary>
    public const string WordCharacters = "A-Za-z\u00AA\u00BA\u00C0-\u00D6\u00D8-\u00F6\u00F8-\u02B8\u02E0-\u02E4\u1D00-\u1D25\u1D2C-\u1D5C\u1D62-\u1D65\u1D6B-\u1D77\u1D79-\u1DBE\u1E00-\u1EFF\u2071\u207F\u2090-\u209C\u212A\u212B\u2132\u214E\u2160-\u2188\u2C60-\u2C7F\uA722-\uA787\uA78B-\uA7AD\uA7B0-\uA7B7\uA7F7-\uA7FF\uAB30-\uAB5A\uAB5C-\uAB64\uFB00-\uFB06\uFF21-\uFF3A\uFF41-\uFF5A";

    private static PipelineFunction _trimmer;
    private static PipelineFunction _stemmer;
    private static PipelineFunction _stopWordFilter;

    /// <summary>
    /// Gets the French trimmer pipeline function.
    /// </summary>
    public static PipelineFunction Trimmer
    {
        get
        {
            if(_trimmer == null)
            {
                _trimmer = TrimmerSupport.GenerateTrimmer(WordCharacters);
                Pipeline.RegisterFunction(_trimmer, "trimmer-fr");
            }
            return _trimmer;
        }
    }

    /// <summary>
    /// Gets the French stemmer pipeline function.
    /// </summary>
    public static PipelineFunction Stemmer
    {
        get
        {
            if(_stemmer == null)
            {
                _stemmer = CreateStemmer();
                Pipeline.RegisterFunction(_stemmer, "stemmer-fr");
            }
            return _stemmer;
        }
    }

    /// <summary>
    /// Gets the French stop word filter pipeline function.
    /// </summary>
    public static PipelineFunction StopWordFilter
    {
        get
        {
            if(_stopWordFilter == null)
            {
                _stopWordFilter = Lunr.StopWordFilter.GenerateStopWordFilter(StopWords);
                Pipeline.RegisterFunction(_stopWordFilter, "stopWordFilter-fr");
            }
            return _stopWordFilter;
        }
    }

    /// <summary>
    /// French stop words.
    /// </summary>
    public static readonly HashSet<string> StopWords =
    [
        "ai", "aie", "aient", "aies", "ait", "as", "au", "aura", "aurai", "auraient",
        "aurais", "aurait", "auras", "aurez", "auriez", "aurions", "aurons", "auront", "aux", "avaient",
        "avais", "avait", "avec", "avez", "aviez", "avions", "avons", "ayant", "ayez", "ayons",
        "c", "ce", "ceci", "celà", "ces", "cet", "cette", "d", "dans", "de",
        "des", "du", "elle", "en", "es", "est", "et", "eu", "eue", "eues",
        "eurent", "eus", "eusse", "eussent", "eusses", "eussiez", "eussions", "eut", "eux", "eûmes",
        "eût", "eûtes", "furent", "fus", "fusse", "fussent", "fusses", "fussiez", "fussions", "fut",
        "fûmes", "fût", "fûtes", "ici", "il", "ils", "j", "je", "l", "la",
        "le", "les", "leur", "leurs", "lui", "m", "ma", "mais", "me", "mes",
        "moi", "mon", "même", "n", "ne", "nos", "notre", "nous", "on", "ont",
        "ou", "par", "pas", "pour", "qu", "que", "quel", "quelle", "quelles", "quels",
        "qui", "s", "sa", "sans", "se", "sera", "serai", "seraient", "serais", "serait",
        "seras", "serez", "seriez", "serions", "serons", "seront", "ses", "soi", "soient", "sois",
        "soit", "sommes", "son", "sont", "soyez", "soyons", "suis", "sur", "t", "ta",
        "te", "tes", "toi", "ton", "tu", "un", "une", "vos", "votre", "vous",
        "y", "à", "étaient", "étais", "était", "étant", "étiez", "étions", "été", "étée",
        "étées", "étés", "êtes"
    ];

    private static PipelineFunction CreateStemmer()
    {
        return (token, index, tokens) =>
        {
            if(token == null)
            {
                return null;
            }

            return token.Update((word, metadata) =>
            {
                var stemmer = new FrenchStemmer();
                stemmer.SetCurrent(word);
                stemmer.Stem();
                return stemmer.Current;
            });
        };
    }

    /// <summary>
    /// Configures a Builder to use French language processing.
    /// </summary>
    /// <param name="builder">The builder to configure.</param>
    public static void Register(Builder builder)
    {
        if(builder == null)
            throw new ArgumentNullException(nameof(builder));

        builder.Pipeline.Reset();
        builder.Pipeline.Add(Trimmer);
        builder.Pipeline.Add(StopWordFilter);
        builder.Pipeline.Add(Stemmer);

        builder.SearchPipeline.Reset();
        builder.SearchPipeline.Add(Stemmer);
    }

    private sealed class FrenchStemmer : SnowballProgram
    {
        private static readonly Among[] A0 = [
            new("col", -1, -1),
            new("par", -1, -1),
            new("tap", -1, -1)
        ];

        private static readonly Among[] A1 = [
            new("", -1, 4),
            new("I", 0, 1),
            new("U", 0, 2),
            new("Y", 0, 3)
        ];

        private static readonly Among[] A2 = [
            new("iqU", -1, 3),
            new("abl", -1, 3),
            new("I\u00E8r", -1, 4),
            new("i\u00E8r", -1, 4),
            new("eus", -1, 2),
            new("iv", -1, 1)
        ];

        private static readonly Among[] A3 = [
            new("ic", -1, 2),
            new("abil", -1, 1),
            new("iv", -1, 3)
        ];

        private static readonly Among[] A4 = [
            new("iqUe", -1, 1),
            new("atrice", -1, 2),
            new("ance", -1, 1),
            new("ence", -1, 5),
            new("logie", -1, 3),
            new("able", -1, 1),
            new("isme", -1, 1),
            new("euse", -1, 11),
            new("iste", -1, 1),
            new("ive", -1, 8),
            new("if", -1, 8),
            new("usion", -1, 4),
            new("ation", -1, 2),
            new("ution", -1, 4),
            new("ateur", -1, 2),
            new("iqUes", -1, 1),
            new("atrices", -1, 2),
            new("ances", -1, 1),
            new("ences", -1, 5),
            new("logies", -1, 3),
            new("ables", -1, 1),
            new("ismes", -1, 1),
            new("euses", -1, 11),
            new("istes", -1, 1),
            new("ives", -1, 8),
            new("ifs", -1, 8),
            new("usions", -1, 4),
            new("ations", -1, 2),
            new("utions", -1, 4),
            new("ateurs", -1, 2),
            new("ments", -1, 15),
            new("ements", 30, 6),
            new("issements", 31, 12),
            new("it\u00E9s", -1, 7),
            new("ment", -1, 15),
            new("ement", 34, 6),
            new("issement", 35, 12),
            new("amment", 34, 13),
            new("emment", 34, 14),
            new("aux", -1, 10),
            new("eaux", 39, 9),
            new("eux", -1, 1),
            new("it\u00E9", -1, 7)
        ];

        private static readonly Among[] A5 = [
            new("ira", -1, 1),
            new("ie", -1, 1),
            new("isse", -1, 1),
            new("issante", -1, 1),
            new("i", -1, 1),
            new("irai", 4, 1),
            new("ir", -1, 1),
            new("iras", -1, 1),
            new("ies", -1, 1),
            new("\u00EEmes", -1, 1),
            new("isses", -1, 1),
            new("issantes", -1, 1),
            new("\u00EEtes", -1, 1),
            new("is", -1, 1),
            new("irais", 13, 1),
            new("issais", 13, 1),
            new("irions", -1, 1),
            new("issions", -1, 1),
            new("irons", -1, 1),
            new("issons", -1, 1),
            new("issants", -1, 1),
            new("it", -1, 1),
            new("irait", 21, 1),
            new("issait", 21, 1),
            new("issant", -1, 1),
            new("iraIent", -1, 1),
            new("issaIent", -1, 1),
            new("irent", -1, 1),
            new("issent", -1, 1),
            new("iront", -1, 1),
            new("\u00EEt", -1, 1),
            new("iriez", -1, 1),
            new("issiez", -1, 1),
            new("irez", -1, 1),
            new("issez", -1, 1)
        ];

        private static readonly Among[] A6 = [
            new("a", -1, 3),
            new("era", 0, 2),
            new("asse", -1, 3),
            new("ante", -1, 3),
            new("\u00E9e", -1, 2),
            new("ai", -1, 3),
            new("erai", 5, 2),
            new("er", -1, 2),
            new("as", -1, 3),
            new("eras", 8, 2),
            new("\u00E2mes", -1, 3),
            new("asses", -1, 3),
            new("antes", -1, 3),
            new("\u00E2tes", -1, 3),
            new("\u00E9es", -1, 2),
            new("ais", -1, 3),
            new("erais", 15, 2),
            new("ions", -1, 1),
            new("erions", 17, 2),
            new("assions", 17, 3),
            new("erons", -1, 2),
            new("ants", -1, 3),
            new("\u00E9s", -1, 2),
            new("ait", -1, 3),
            new("erait", 23, 2),
            new("ant", -1, 3),
            new("aIent", -1, 3),
            new("eraIent", 26, 2),
            new("\u00E8rent", -1, 2),
            new("assent", -1, 3),
            new("eront", -1, 2),
            new("\u00E2t", -1, 3),
            new("ez", -1, 2),
            new("iez", 32, 2),
            new("eriez", 33, 2),
            new("assiez", 33, 3),
            new("erez", 32, 2),
            new("\u00E9", -1, 2)
        ];

        private static readonly Among[] A7 = [
            new("e", -1, 3),
            new("I\u00E8re", 0, 2),
            new("i\u00E8re", 0, 2),
            new("ion", -1, 1),
            new("Ier", -1, 2),
            new("ier", -1, 2),
            new("\u00EB", -1, 4)
        ];

        private static readonly Among[] A8 = [
            new("ell", -1, -1),
            new("eill", -1, -1),
            new("enn", -1, -1),
            new("onn", -1, -1),
            new("ett", -1, -1)
        ];

        private static readonly int[] GV = [17, 65, 16, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 130, 103, 8, 5];

        private static readonly int[] GKeepWithS = [1, 65, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128];

        private int IPV;
        private int IP1;
        private int IP2;

        public void Stem()
        {
            int v1 = Cursor;
            RPrelude();
            Cursor = v1;
            RMarkRegions();
            LimitBackward = v1;
            Cursor = Limit;
            Habr5();
            Cursor = Limit;
            RUnDouble();
            Cursor = Limit;
            RUnAccent();
            Cursor = LimitBackward;
            RPostlude();
        }

        private bool Habr1(string c1, string c2, int v1)
        {
            if(EqS(1, c1))
            {
                Ket = Cursor;
                if(InGrouping(GV, 97, 251))
                {
                    SliceFrom(c2);
                    Cursor = v1;
                    return true;
                }
            }
            return false;
        }

        private bool Habr2(string c1, string c2, int v1)
        {
            if(EqS(1, c1))
            {
                Ket = Cursor;
                SliceFrom(c2);
                Cursor = v1;
                return true;
            }
            return false;
        }

        private void RPrelude()
        {
            int v1, v2;
            while(true)
            {
                v1 = Cursor;
                if(InGrouping(GV, 97, 251))
                {
                    Bra = Cursor;
                    v2 = Cursor;
                    if(Habr1("u", "U", v1))
                        continue;
                    Cursor = v2;
                    if(Habr1("i", "I", v1))
                        continue;
                    Cursor = v2;
                    if(Habr2("y", "Y", v1))
                        continue;
                }
                Cursor = v1;
                Bra = v1;
                if(!Habr1("y", "Y", v1))
                {
                    Cursor = v1;
                    if(EqS(1, "q"))
                    {
                        Bra = Cursor;
                        if(Habr2("u", "U", v1))
                            continue;
                    }
                    Cursor = v1;
                    if(v1 >= Limit)
                        return;
                    Cursor++;
                }
            }
        }

        private bool Habr3()
        {
            while(!InGrouping(GV, 97, 251))
            {
                if(Cursor >= Limit)
                    return true;
                Cursor++;
            }
            while(!OutGrouping(GV, 97, 251))
            {
                if(Cursor >= Limit)
                    return true;
                Cursor++;
            }
            return false;
        }

        private void RMarkRegions()
        {
            int v1 = Cursor;
            IPV = Limit;
            IP1 = IPV;
            IP2 = IPV;

            if(InGrouping(GV, 97, 251) && InGrouping(GV, 97, 251) && Cursor < Limit)
            {
                Cursor++;
            }
            else
            {
                Cursor = v1;
                if(FindAmong(A0, 3) == 0)
                {
                    Cursor = v1;
                    do
                    {
                        if(Cursor >= Limit)
                        {
                            Cursor = IPV;
                            break;
                        }
                        Cursor++;
                    } while(!InGrouping(GV, 97, 251));
                }
            }
            IPV = Cursor;
            Cursor = v1;
            if(!Habr3())
            {
                IP1 = Cursor;
                if(!Habr3())
                    IP2 = Cursor;
            }
        }

        private void RPostlude()
        {
            int amongVar, v1;
            while(true)
            {
                v1 = Cursor;
                Bra = v1;
                amongVar = FindAmong(A1, 4);
                if(amongVar == 0)
                    break;
                Ket = Cursor;
                switch(amongVar)
                {
                    case 1:
                        SliceFrom("i");
                        break;
                    case 2:
                        SliceFrom("u");
                        break;
                    case 3:
                        SliceFrom("y");
                        break;
                    case 4:
                        if(Cursor >= Limit)
                            return;
                        Cursor++;
                        break;
                }
            }
        }

        private bool RRV()
        {
            return IPV <= Cursor;
        }

        private bool RR1()
        {
            return IP1 <= Cursor;
        }

        private bool RR2()
        {
            return IP2 <= Cursor;
        }

        private bool RStandardSuffix()
        {
            int amongVar, v1;
            Ket = Cursor;
            amongVar = FindAmongB(A4, 43);
            if(amongVar != 0)
            {
                Bra = Cursor;
                switch(amongVar)
                {
                    case 1:
                        if(!RR2())
                            return false;
                        SliceDel();
                        break;
                    case 2:
                        if(!RR2())
                            return false;
                        SliceDel();
                        Ket = Cursor;
                        if(EqSB(2, "ic"))
                        {
                            Bra = Cursor;
                            if(!RR2())
                                SliceFrom("iqU");
                            else
                                SliceDel();
                        }
                        break;
                    case 3:
                        if(!RR2())
                            return false;
                        SliceFrom("log");
                        break;
                    case 4:
                        if(!RR2())
                            return false;
                        SliceFrom("u");
                        break;
                    case 5:
                        if(!RR2())
                            return false;
                        SliceFrom("ent");
                        break;
                    case 6:
                        if(!RRV())
                            return false;
                        SliceDel();
                        Ket = Cursor;
                        amongVar = FindAmongB(A2, 6);
                        if(amongVar != 0)
                        {
                            Bra = Cursor;
                            switch(amongVar)
                            {
                                case 1:
                                    if(RR2())
                                    {
                                        SliceDel();
                                        Ket = Cursor;
                                        if(EqSB(2, "at"))
                                        {
                                            Bra = Cursor;
                                            if(RR2())
                                                SliceDel();
                                        }
                                    }
                                    break;
                                case 2:
                                    if(RR2())
                                        SliceDel();
                                    else if(RR1())
                                        SliceFrom("eux");
                                    break;
                                case 3:
                                    if(RR2())
                                        SliceDel();
                                    break;
                                case 4:
                                    if(RRV())
                                        SliceFrom("i");
                                    break;
                            }
                        }
                        break;
                    case 7:
                        if(!RR2())
                            return false;
                        SliceDel();
                        Ket = Cursor;
                        amongVar = FindAmongB(A3, 3);
                        if(amongVar != 0)
                        {
                            Bra = Cursor;
                            switch(amongVar)
                            {
                                case 1:
                                    if(RR2())
                                        SliceDel();
                                    else
                                        SliceFrom("abl");
                                    break;
                                case 2:
                                    if(RR2())
                                        SliceDel();
                                    else
                                        SliceFrom("iqU");
                                    break;
                                case 3:
                                    if(RR2())
                                        SliceDel();
                                    break;
                            }
                        }
                        break;
                    case 8:
                        if(!RR2())
                            return false;
                        SliceDel();
                        Ket = Cursor;
                        if(EqSB(2, "at"))
                        {
                            Bra = Cursor;
                            if(RR2())
                            {
                                SliceDel();
                                Ket = Cursor;
                                if(EqSB(2, "ic"))
                                {
                                    Bra = Cursor;
                                    if(RR2())
                                        SliceDel();
                                    else
                                        SliceFrom("iqU");
                                }
                            }
                        }
                        break;
                    case 9:
                        SliceFrom("eau");
                        break;
                    case 10:
                        if(!RR1())
                            return false;
                        SliceFrom("al");
                        break;
                    case 11:
                        if(RR2())
                            SliceDel();
                        else if(!RR1())
                            return false;
                        else
                            SliceFrom("eux");
                        break;
                    case 12:
                        if(!RR1() || !OutGroupingB(GV, 97, 251))
                            return false;
                        SliceDel();
                        break;
                    case 13:
                        if(RRV())
                            SliceFrom("ant");
                        return false;
                    case 14:
                        if(RRV())
                            SliceFrom("ent");
                        return false;
                    case 15:
                        v1 = Limit - Cursor;
                        if(InGroupingB(GV, 97, 251) && RRV())
                        {
                            Cursor = Limit - v1;
                            SliceDel();
                        }
                        return false;
                }
                return true;
            }
            return false;
        }

        private bool RIVerbSuffix()
        {
            int amongVar, v1;
            if(Cursor < IPV)
                return false;
            v1 = LimitBackward;
            LimitBackward = IPV;
            Ket = Cursor;
            amongVar = FindAmongB(A5, 35);
            if(amongVar == 0)
            {
                LimitBackward = v1;
                return false;
            }
            Bra = Cursor;
            if(amongVar == 1)
            {
                if(!OutGroupingB(GV, 97, 251))
                {
                    LimitBackward = v1;
                    return false;
                }
                SliceDel();
            }
            LimitBackward = v1;
            return true;
        }

        private bool RVerbSuffix()
        {
            int amongVar, v2, v3;
            if(Cursor < IPV)
                return false;
            v2 = LimitBackward;
            LimitBackward = IPV;
            Ket = Cursor;
            amongVar = FindAmongB(A6, 38);
            if(amongVar == 0)
            {
                LimitBackward = v2;
                return false;
            }
            Bra = Cursor;
            switch(amongVar)
            {
                case 1:
                    if(!RR2())
                    {
                        LimitBackward = v2;
                        return false;
                    }
                    SliceDel();
                    break;
                case 2:
                    SliceDel();
                    break;
                case 3:
                    SliceDel();
                    v3 = Limit - Cursor;
                    Ket = Cursor;
                    if(EqSB(1, "e"))
                    {
                        Bra = Cursor;
                        SliceDel();
                    }
                    else
                        Cursor = Limit - v3;
                    break;
            }
            LimitBackward = v2;
            return true;
        }

        private void RResidualSuffix()
        {
            int amongVar, v1 = Limit - Cursor;
            int v2, v4, v5;

            Ket = Cursor;
            if(EqSB(1, "s"))
            {
                Bra = Cursor;
                v2 = Limit - Cursor;
                if(OutGroupingB(GKeepWithS, 97, 232))
                {
                    Cursor = Limit - v2;
                    SliceDel();
                }
                else
                    Cursor = Limit - v1;
            }
            else
                Cursor = Limit - v1;

            if(Cursor >= IPV)
            {
                v4 = LimitBackward;
                LimitBackward = IPV;
                Ket = Cursor;
                amongVar = FindAmongB(A7, 7);
                if(amongVar != 0)
                {
                    Bra = Cursor;
                    switch(amongVar)
                    {
                        case 1:
                            if(RR2())
                            {
                                v5 = Limit - Cursor;
                                if(!EqSB(1, "s"))
                                {
                                    Cursor = Limit - v5;
                                    if(!EqSB(1, "t"))
                                        break;
                                }
                                SliceDel();
                            }
                            break;
                        case 2:
                            SliceFrom("i");
                            break;
                        case 3:
                            SliceDel();
                            break;
                        case 4:
                            if(EqSB(2, "gu"))
                                SliceDel();
                            break;
                    }
                }
                LimitBackward = v4;
            }
        }

        private void RUnDouble()
        {
            int v1 = Limit - Cursor;
            if(FindAmongB(A8, 5) != 0)
            {
                Cursor = Limit - v1;
                Ket = Cursor;
                if(Cursor > LimitBackward)
                {
                    Cursor--;
                    Bra = Cursor;
                    SliceDel();
                }
            }
        }

        private void RUnAccent()
        {
            int v1, v2 = 1;
            while(OutGroupingB(GV, 97, 251))
                v2--;
            if(v2 <= 0)
            {
                Ket = Cursor;
                v1 = Limit - Cursor;
                if(!EqSB(1, "\u00E9"))
                {
                    Cursor = Limit - v1;
                    if(!EqSB(1, "\u00E8"))
                        return;
                }
                Bra = Cursor;
                SliceFrom("e");
            }
        }

        private void Habr5()
        {
            if(!RStandardSuffix())
            {
                Cursor = Limit;
                if(!RIVerbSuffix())
                {
                    Cursor = Limit;
                    if(!RVerbSuffix())
                    {
                        Cursor = Limit;
                        RResidualSuffix();
                        return;
                    }
                }
            }
            Cursor = Limit;
            Ket = Cursor;
            if(EqSB(1, "Y"))
            {
                Bra = Cursor;
                SliceFrom("i");
            }
            else
            {
                Cursor = Limit;
                if(EqSB(1, "\u00E7"))
                {
                    Bra = Cursor;
                    SliceFrom("c");
                }
            }
        }
    }
}
