/*!
 * Lunr languages, `Italian` language
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
/// Italian language support for Lunr.
/// Provides stemming and stop word filtering for Italian text.
/// </summary>
public static class Italian
{
    /// <summary>
    /// Word characters specific to Italian language.
    /// </summary>
    public const string WordCharacters = "A-Za-z\u00AA\u00BA\u00C0-\u00D6\u00D8-\u00F6\u00F8-\u02B8\u02E0-\u02E4\u1D00-\u1D25\u1D2C-\u1D5C\u1D62-\u1D65\u1D6B-\u1D77\u1D79-\u1DBE\u1E00-\u1EFF\u2071\u207F\u2090-\u209C\u212A\u212B\u2132\u214E\u2160-\u2188\u2C60-\u2C7F\uA722-\uA787\uA78B-\uA7AD\uA7B0-\uA7B7\uA7F7-\uA7FF\uAB30-\uAB5A\uAB5C-\uAB64\uFB00-\uFB06\uFF21-\uFF3A\uFF41-\uFF5A";

    private static PipelineFunction _trimmer;
    private static PipelineFunction _stemmer;
    private static PipelineFunction _stopWordFilter;

    /// <summary>
    /// Gets the Italian trimmer pipeline function.
    /// </summary>
    public static PipelineFunction Trimmer
    {
        get
        {
            if(_trimmer == null)
            {
                _trimmer = TrimmerSupport.GenerateTrimmer(WordCharacters);
                Pipeline.RegisterFunction(_trimmer, "trimmer-it");
            }
            return _trimmer;
        }
    }

    /// <summary>
    /// Gets the Italian stemmer pipeline function.
    /// </summary>
    public static PipelineFunction Stemmer
    {
        get
        {
            if(_stemmer == null)
            {
                _stemmer = CreateStemmer();
                Pipeline.RegisterFunction(_stemmer, "stemmer-it");
            }
            return _stemmer;
        }
    }

    /// <summary>
    /// Gets the Italian stop word filter pipeline function.
    /// </summary>
    public static PipelineFunction StopWordFilter
    {
        get
        {
            if(_stopWordFilter == null)
            {
                _stopWordFilter = Lunr.StopWordFilter.GenerateStopWordFilter(StopWords);
                Pipeline.RegisterFunction(_stopWordFilter, "stopWordFilter-it");
            }
            return _stopWordFilter;
        }
    }

    /// <summary>
    /// Italian stop words.
    /// </summary>
    public static readonly HashSet<string> StopWords =
    [
        "a", "abbia", "abbiamo", "abbiano", "abbiate", "ad", "agl", "agli", "ai", "al",
        "all", "alla", "alle", "allo", "anche", "avemmo", "avendo", "avesse", "avessero", "avessi",
        "avessimo", "aveste", "avesti", "avete", "aveva", "avevamo", "avevano", "avevate", "avevi", "avevo",
        "avrai", "avranno", "avrebbe", "avrebbero", "avrei", "avremmo", "avremo", "avreste", "avresti", "avrete",
        "avrà", "avrò", "avuta", "avute", "avuti", "avuto", "c", "che", "chi", "ci",
        "coi", "col", "come", "con", "contro", "cui", "da", "dagl", "dagli", "dai",
        "dal", "dall", "dalla", "dalle", "dallo", "degl", "degli", "dei", "del", "dell",
        "della", "delle", "dello", "di", "dov", "dove", "e", "ebbe", "ebbero", "ebbi",
        "ed", "era", "erano", "eravamo", "eravate", "eri", "ero", "essendo", "faccia", "facciamo",
        "facciano", "facciate", "faccio", "facemmo", "facendo", "facesse", "facessero", "facessi", "facessimo", "faceste",
        "facesti", "faceva", "facevamo", "facevano", "facevate", "facevi", "facevo", "fai", "fanno", "farai",
        "faranno", "farebbe", "farebbero", "farei", "faremmo", "faremo", "fareste", "faresti", "farete", "farà",
        "farò", "fece", "fecero", "feci", "fosse", "fossero", "fossi", "fossimo", "foste", "fosti",
        "fu", "fui", "fummo", "furono", "gli", "ha", "hai", "hanno", "ho", "i",
        "il", "in", "io", "l", "la", "le", "lei", "li", "lo", "loro",
        "lui", "ma", "mi", "mia", "mie", "miei", "mio", "ne", "negl", "negli",
        "nei", "nel", "nell", "nella", "nelle", "nello", "noi", "non", "nostra", "nostre",
        "nostri", "nostro", "o", "per", "perché", "più", "quale", "quanta", "quante", "quanti",
        "quanto", "quella", "quelle", "quelli", "quello", "questa", "queste", "questi", "questo", "sarai",
        "saranno", "sarebbe", "sarebbero", "sarei", "saremmo", "saremo", "sareste", "saresti", "sarete", "sarà",
        "sarò", "se", "sei", "si", "sia", "siamo", "siano", "siate", "siete", "sono",
        "sta", "stai", "stando", "stanno", "starai", "staranno", "starebbe", "starebbero", "starei", "staremmo",
        "staremo", "stareste", "staresti", "starete", "starà", "starò", "stava", "stavamo", "stavano", "stavate",
        "stavi", "stavo", "stemmo", "stesse", "stessero", "stessi", "stessimo", "steste", "stesti", "stette",
        "stettero", "stetti", "stia", "stiamo", "stiano", "stiate", "sto", "su", "sua", "sue",
        "sugl", "sugli", "sui", "sul", "sull", "sulla", "sulle", "sullo", "suo", "suoi",
        "ti", "tra", "tu", "tua", "tue", "tuo", "tuoi", "tutti", "tutto", "un",
        "una", "uno", "vi", "voi", "vostra", "vostre", "vostri", "vostro", "è"
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
                var stemmer = new ItalianStemmer();
                stemmer.SetCurrent(word);
                stemmer.Stem();
                return stemmer.Current;
            });
        };
    }

    /// <summary>
    /// Configures a Builder to use Italian language processing.
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

    private sealed class ItalianStemmer : SnowballProgram
    {
        private static readonly Among[] A0 = [
            new("", -1, 7),
            new("qu", 0, 6),
            new("\u00E1", 0, 1),
            new("\u00E9", 0, 2),
            new("\u00ED", 0, 3),
            new("\u00F3", 0, 4),
            new("\u00FA", 0, 5)
        ];

        private static readonly Among[] A1 = [
            new("", -1, 3),
            new("I", 0, 1),
            new("U", 0, 2)
        ];

        private static readonly Among[] A2 = [
            new("la", -1, -1),
            new("cela", 0, -1),
            new("gliela", 0, -1),
            new("mela", 0, -1),
            new("tela", 0, -1),
            new("vela", 0, -1),
            new("le", -1, -1),
            new("cele", 6, -1),
            new("gliele", 6, -1),
            new("mele", 6, -1),
            new("tele", 6, -1),
            new("vele", 6, -1),
            new("ne", -1, -1),
            new("cene", 12, -1),
            new("gliene", 12, -1),
            new("mene", 12, -1),
            new("sene", 12, -1),
            new("tene", 12, -1),
            new("vene", 12, -1),
            new("ci", -1, -1),
            new("li", -1, -1),
            new("celi", 20, -1),
            new("glieli", 20, -1),
            new("meli", 20, -1),
            new("teli", 20, -1),
            new("veli", 20, -1),
            new("gli", 20, -1),
            new("mi", -1, -1),
            new("si", -1, -1),
            new("ti", -1, -1),
            new("vi", -1, -1),
            new("lo", -1, -1),
            new("celo", 31, -1),
            new("glielo", 31, -1),
            new("melo", 31, -1),
            new("telo", 31, -1),
            new("velo", 31, -1)
        ];

        private static readonly Among[] A3 = [
            new("ando", -1, 1),
            new("endo", -1, 1),
            new("ar", -1, 2),
            new("er", -1, 2),
            new("ir", -1, 2)
        ];

        private static readonly Among[] A4 = [
            new("ic", -1, -1),
            new("abil", -1, -1),
            new("os", -1, -1),
            new("iv", -1, 1)
        ];

        private static readonly Among[] A5 = [
            new("ic", -1, 1),
            new("abil", -1, 1),
            new("iv", -1, 1)
        ];

        private static readonly Among[] A6 = [
            new("ica", -1, 1),
            new("logia", -1, 3),
            new("osa", -1, 1),
            new("ista", -1, 1),
            new("iva", -1, 9),
            new("anza", -1, 1),
            new("enza", -1, 5),
            new("ice", -1, 1),
            new("atrice", 7, 1),
            new("iche", -1, 1),
            new("logie", -1, 3),
            new("abile", -1, 1),
            new("ibile", -1, 1),
            new("usione", -1, 4),
            new("azione", -1, 2),
            new("uzione", -1, 4),
            new("atore", -1, 2),
            new("ose", -1, 1),
            new("ante", -1, 1),
            new("mente", -1, 1),
            new("amente", 19, 7),
            new("iste", -1, 1),
            new("ive", -1, 9),
            new("anze", -1, 1),
            new("enze", -1, 5),
            new("ici", -1, 1),
            new("atrici", 25, 1),
            new("ichi", -1, 1),
            new("abili", -1, 1),
            new("ibili", -1, 1),
            new("ismi", -1, 1),
            new("usioni", -1, 4),
            new("azioni", -1, 2),
            new("uzioni", -1, 4),
            new("atori", -1, 2),
            new("osi", -1, 1),
            new("anti", -1, 1),
            new("amente", -1, 6),
            new("imenti", -1, 6),
            new("isti", -1, 1),
            new("ivi", -1, 9),
            new("ico", -1, 1),
            new("ismo", -1, 1),
            new("oso", -1, 1),
            new("amento", -1, 6),
            new("imento", -1, 6),
            new("ivo", -1, 9),
            new("it\u00E0", -1, 8),
            new("ist\u00E0", -1, 1),
            new("ist\u00E8", -1, 1),
            new("ist\u00EC", -1, 1)
        ];

        private static readonly Among[] A7 = [
            new("isca", -1, 1),
            new("enda", -1, 1),
            new("ata", -1, 1),
            new("ita", -1, 1),
            new("uta", -1, 1),
            new("ava", -1, 1),
            new("eva", -1, 1),
            new("iva", -1, 1),
            new("erebbe", -1, 1),
            new("irebbe", -1, 1),
            new("isce", -1, 1),
            new("ende", -1, 1),
            new("are", -1, 1),
            new("ere", -1, 1),
            new("ire", -1, 1),
            new("asse", -1, 1),
            new("ate", -1, 1),
            new("avate", 16, 1),
            new("evate", 16, 1),
            new("ivate", 16, 1),
            new("ete", -1, 1),
            new("erete", 20, 1),
            new("irete", 20, 1),
            new("ite", -1, 1),
            new("ereste", -1, 1),
            new("ireste", -1, 1),
            new("ute", -1, 1),
            new("erai", -1, 1),
            new("irai", -1, 1),
            new("isci", -1, 1),
            new("endi", -1, 1),
            new("erei", -1, 1),
            new("irei", -1, 1),
            new("assi", -1, 1),
            new("ati", -1, 1),
            new("iti", -1, 1),
            new("eresti", -1, 1),
            new("iresti", -1, 1),
            new("uti", -1, 1),
            new("avi", -1, 1),
            new("evi", -1, 1),
            new("ivi", -1, 1),
            new("isco", -1, 1),
            new("ando", -1, 1),
            new("endo", -1, 1),
            new("Yamo", -1, 1),
            new("iamo", -1, 1),
            new("avamo", -1, 1),
            new("evamo", -1, 1),
            new("ivamo", -1, 1),
            new("eremo", -1, 1),
            new("iremo", -1, 1),
            new("assimo", -1, 1),
            new("ammo", -1, 1),
            new("emmo", -1, 1),
            new("eremmo", 54, 1),
            new("iremmo", 54, 1),
            new("immo", -1, 1),
            new("ano", -1, 1),
            new("iscano", 58, 1),
            new("avano", 58, 1),
            new("evano", 58, 1),
            new("ivano", 58, 1),
            new("eranno", -1, 1),
            new("iranno", -1, 1),
            new("ono", -1, 1),
            new("iscono", 65, 1),
            new("arono", 65, 1),
            new("erono", 65, 1),
            new("irono", 65, 1),
            new("erebbero", -1, 1),
            new("irebbero", -1, 1),
            new("assero", -1, 1),
            new("essero", -1, 1),
            new("issero", -1, 1),
            new("ato", -1, 1),
            new("ito", -1, 1),
            new("uto", -1, 1),
            new("avo", -1, 1),
            new("evo", -1, 1),
            new("ivo", -1, 1),
            new("ar", -1, 1),
            new("ir", -1, 1),
            new("er\u00E0", -1, 1),
            new("ir\u00E0", -1, 1),
            new("er\u00F2", -1, 1),
            new("ir\u00F2", -1, 1)
        ];

        private static readonly int[] GV = [17, 65, 16, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 128, 8, 2, 1];

        private static readonly int[] GAEIO = [17, 65, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 128, 8, 2];

        private static readonly int[] GCG = [17];

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
            RAttachedPronoun();
            Cursor = Limit;
            if(!RStandardSuffix())
            {
                Cursor = Limit;
                RVerbSuffix();
            }
            Cursor = Limit;
            RVowelSuffix();
            Cursor = LimitBackward;
            RPostlude();
        }

        private bool Habr1(string c1, string c2, int v1)
        {
            if(EqS(1, c1))
            {
                Ket = Cursor;
                if(InGrouping(GV, 97, 249))
                {
                    SliceFrom(c2);
                    Cursor = v1;
                    return true;
                }
            }
            return false;
        }

        private void RPrelude()
        {
            int amongVar, v1 = Cursor;
            int v2, v3, v4;

            while(true)
            {
                Bra = Cursor;
                amongVar = FindAmong(A0, 7);
                if(amongVar != 0)
                {
                    Ket = Cursor;
                    switch(amongVar)
                    {
                        case 1:
                            SliceFrom("\u00E0");
                            continue;
                        case 2:
                            SliceFrom("\u00E8");
                            continue;
                        case 3:
                            SliceFrom("\u00EC");
                            continue;
                        case 4:
                            SliceFrom("\u00F2");
                            continue;
                        case 5:
                            SliceFrom("\u00F9");
                            continue;
                        case 6:
                            SliceFrom("qU");
                            continue;
                        case 7:
                            if(Cursor >= Limit)
                                break;
                            Cursor++;
                            continue;
                    }
                }
                break;
            }

            Cursor = v1;
            while(true)
            {
                v2 = Cursor;
                while(true)
                {
                    v3 = Cursor;
                    if(InGrouping(GV, 97, 249))
                    {
                        Bra = Cursor;
                        v4 = Cursor;
                        if(Habr1("u", "U", v3))
                            break;
                        Cursor = v4;
                        if(Habr1("i", "I", v3))
                            break;
                    }
                    Cursor = v3;
                    if(Cursor >= Limit)
                    {
                        Cursor = v2;
                        return;
                    }
                    Cursor++;
                }
            }
        }

        private bool Habr2(int v1)
        {
            Cursor = v1;
            if(!InGrouping(GV, 97, 249))
                return false;
            while(!OutGrouping(GV, 97, 249))
            {
                if(Cursor >= Limit)
                    return false;
                Cursor++;
            }
            return true;
        }

        private bool Habr3()
        {
            if(InGrouping(GV, 97, 249))
            {
                int v1 = Cursor;
                if(OutGrouping(GV, 97, 249))
                {
                    while(!InGrouping(GV, 97, 249))
                    {
                        if(Cursor >= Limit)
                            return Habr2(v1);
                        Cursor++;
                    }
                    return true;
                }
                return Habr2(v1);
            }
            return false;
        }

        private void Habr4()
        {
            int v1 = Cursor;
            int v2;

            if(!Habr3())
            {
                Cursor = v1;
                if(!OutGrouping(GV, 97, 249))
                    return;
                v2 = Cursor;
                if(OutGrouping(GV, 97, 249))
                {
                    while(!InGrouping(GV, 97, 249))
                    {
                        if(Cursor >= Limit)
                        {
                            Cursor = v2;
                            if(InGrouping(GV, 97, 249) && Cursor < Limit)
                                Cursor++;
                            return;
                        }
                        Cursor++;
                    }
                    IPV = Cursor;
                    return;
                }
                Cursor = v2;
                if(!InGrouping(GV, 97, 249) || Cursor >= Limit)
                    return;
                Cursor++;
            }
            IPV = Cursor;
        }

        private bool Habr5()
        {
            while(!InGrouping(GV, 97, 249))
            {
                if(Cursor >= Limit)
                    return false;
                Cursor++;
            }
            while(!OutGrouping(GV, 97, 249))
            {
                if(Cursor >= Limit)
                    return false;
                Cursor++;
            }
            return true;
        }

        private void RMarkRegions()
        {
            int v1 = Cursor;
            IPV = Limit;
            IP1 = IPV;
            IP2 = IPV;
            Habr4();
            Cursor = v1;
            if(Habr5())
            {
                IP1 = Cursor;
                if(Habr5())
                    IP2 = Cursor;
            }
        }

        private void RPostlude()
        {
            int amongVar;
            while(true)
            {
                Bra = Cursor;
                amongVar = FindAmong(A1, 3);
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

        private void RAttachedPronoun()
        {
            int amongVar;
            Ket = Cursor;
            if(FindAmongB(A2, 37) != 0)
            {
                Bra = Cursor;
                amongVar = FindAmongB(A3, 5);
                if(amongVar != 0 && RRV())
                {
                    switch(amongVar)
                    {
                        case 1:
                            SliceDel();
                            break;
                        case 2:
                            SliceFrom("e");
                            break;
                    }
                }
            }
        }

        private bool RStandardSuffix()
        {
            int amongVar;
            Ket = Cursor;
            amongVar = FindAmongB(A6, 51);
            if(amongVar == 0)
                return false;
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
                        if(RR2())
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
                    SliceFrom("ente");
                    break;
                case 6:
                    if(!RRV())
                        return false;
                    SliceDel();
                    break;
                case 7:
                    if(!RR1())
                        return false;
                    SliceDel();
                    Ket = Cursor;
                    amongVar = FindAmongB(A4, 4);
                    if(amongVar != 0)
                    {
                        Bra = Cursor;
                        if(RR2())
                        {
                            SliceDel();
                            if(amongVar == 1)
                            {
                                Ket = Cursor;
                                if(EqSB(2, "at"))
                                {
                                    Bra = Cursor;
                                    if(RR2())
                                        SliceDel();
                                }
                            }
                        }
                    }
                    break;
                case 8:
                    if(!RR2())
                        return false;
                    SliceDel();
                    Ket = Cursor;
                    amongVar = FindAmongB(A5, 3);
                    if(amongVar != 0)
                    {
                        Bra = Cursor;
                        if(amongVar == 1)
                            if(RR2())
                                SliceDel();
                    }
                    break;
                case 9:
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
                            }
                        }
                    }
                    break;
            }
            return true;
        }

        private void RVerbSuffix()
        {
            int amongVar, v1;
            if(Cursor >= IPV)
            {
                v1 = LimitBackward;
                LimitBackward = IPV;
                Ket = Cursor;
                amongVar = FindAmongB(A7, 87);
                if(amongVar != 0)
                {
                    Bra = Cursor;
                    if(amongVar == 1)
                        SliceDel();
                }
                LimitBackward = v1;
            }
        }

        private void Habr6()
        {
            int v1 = Limit - Cursor;
            Ket = Cursor;
            if(InGroupingB(GAEIO, 97, 242))
            {
                Bra = Cursor;
                if(RRV())
                {
                    SliceDel();
                    Ket = Cursor;
                    if(EqSB(1, "i"))
                    {
                        Bra = Cursor;
                        if(RRV())
                        {
                            SliceDel();
                            return;
                        }
                    }
                }
            }
            Cursor = Limit - v1;
        }

        private void RVowelSuffix()
        {
            Habr6();
            Ket = Cursor;
            if(EqSB(1, "h"))
            {
                Bra = Cursor;
                if(InGroupingB(GCG, 99, 103))
                    if(RRV())
                        SliceDel();
            }
        }
    }
}
