/*!
 * Lunr languages, `German` language
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
/// German language support for Lunr.
/// Provides stemming and stop word filtering for German text.
/// </summary>
public static class German
{
    /// <summary>
    /// Word characters specific to German language.
    /// </summary>
    public const string WordCharacters = "A-Za-z\u00AA\u00BA\u00C0-\u00D6\u00D8-\u00F6\u00F8-\u02B8\u02E0-\u02E4\u1D00-\u1D25\u1D2C-\u1D5C\u1D62-\u1D65\u1D6B-\u1D77\u1D79-\u1DBE\u1E00-\u1EFF\u2071\u207F\u2090-\u209C\u212A\u212B\u2132\u214E\u2160-\u2188\u2C60-\u2C7F\uA722-\uA787\uA78B-\uA7AD\uA7B0-\uA7B7\uA7F7-\uA7FF\uAB30-\uAB5A\uAB5C-\uAB64\uFB00-\uFB06\uFF21-\uFF3A\uFF41-\uFF5A";

    private static PipelineFunction _trimmer;
    private static PipelineFunction _stemmer;
    private static PipelineFunction _stopWordFilter;

    /// <summary>
    /// Gets the German trimmer pipeline function.
    /// </summary>
    public static PipelineFunction Trimmer
    {
        get
        {
            if(_trimmer == null)
            {
                _trimmer = TrimmerSupport.GenerateTrimmer(WordCharacters);
                Pipeline.RegisterFunction(_trimmer, "trimmer-de");
            }
            return _trimmer;
        }
    }

    /// <summary>
    /// Gets the German stemmer pipeline function.
    /// </summary>
    public static PipelineFunction Stemmer
    {
        get
        {
            if(_stemmer == null)
            {
                _stemmer = CreateStemmer();
                Pipeline.RegisterFunction(_stemmer, "stemmer-de");
            }
            return _stemmer;
        }
    }

    /// <summary>
    /// Gets the German stop word filter pipeline function.
    /// </summary>
    public static PipelineFunction StopWordFilter
    {
        get
        {
            if(_stopWordFilter == null)
            {
                _stopWordFilter = Lunr.StopWordFilter.GenerateStopWordFilter(StopWords);
                Pipeline.RegisterFunction(_stopWordFilter, "stopWordFilter-de");
            }
            return _stopWordFilter;
        }
    }

    /// <summary>
    /// German stop words.
    /// </summary>
    public static readonly HashSet<string> StopWords =
    [
        "aber", "alle", "allem", "allen", "aller", "alles", "als", "also", "am", "an",
        "ander", "andere", "anderem", "anderen", "anderer", "anderes", "anderm", "andern", "anderr", "anders",
        "auch", "auf", "aus", "bei", "bin", "bis", "bist", "da", "damit", "dann",
        "das", "dasselbe", "dazu", "daß", "dein", "deine", "deinem", "deinen", "deiner", "deines",
        "dem", "demselben", "den", "denn", "denselben", "der", "derer", "derselbe", "derselben", "des",
        "desselben", "dessen", "dich", "die", "dies", "diese", "dieselbe", "dieselben", "diesem", "diesen",
        "dieser", "dieses", "dir", "doch", "dort", "du", "durch", "ein", "eine", "einem",
        "einen", "einer", "eines", "einig", "einige", "einigem", "einigen", "einiger", "einiges",
        "einmal", "er", "es", "etwas", "euch", "euer", "eure", "eurem", "euren", "eurer",
        "eures", "für", "gegen", "gewesen", "hab", "habe", "haben", "hat", "hatte", "hatten",
        "hier", "hin", "hinter", "ich", "ihm", "ihn", "ihnen", "ihr", "ihre", "ihrem",
        "ihren", "ihrer", "ihres", "im", "in", "indem", "ins", "ist", "jede", "jedem",
        "jeden", "jeder", "jedes", "jene", "jenem", "jenen", "jener", "jenes", "jetzt", "kann",
        "kein", "keine", "keinem", "keinen", "keiner", "keines", "können", "könnte", "machen", "man",
        "manche", "manchem", "manchen", "mancher", "manches", "mein", "meine", "meinem", "meinen", "meiner",
        "meines", "mich", "mir", "mit", "muss", "musste", "nach", "nicht", "nichts", "noch",
        "nun", "nur", "ob", "oder", "ohne", "sehr", "sein", "seine", "seinem", "seinen",
        "seiner", "seines", "selbst", "sich", "sie", "sind", "so", "solche", "solchem", "solchen",
        "solcher", "solches", "soll", "sollte", "sondern", "sonst", "um", "und", "uns", "unse",
        "unsem", "unsen", "unser", "unses", "unter", "viel", "vom", "von", "vor", "war",
        "waren", "warst", "was", "weg", "weil", "weiter", "welche", "welchem", "welchen", "welcher",
        "welches", "wenn", "werde", "werden", "wie", "wieder", "will", "wir", "wird", "wirst",
        "wo", "wollen", "wollte", "während", "würde", "würden", "zu", "zum", "zur", "zwar",
        "zwischen", "über"
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
                var stemmer = new GermanStemmer();
                stemmer.SetCurrent(word);
                stemmer.Stem();
                return stemmer.Current;
            });
        };
    }

    /// <summary>
    /// Configures a Builder to use German language processing.
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

    private sealed class GermanStemmer : SnowballProgram
    {
        private static readonly Among[] A0 = [
            new("", -1, 6),
            new("U", 0, 2),
            new("Y", 0, 1),
            new("\u00E4", 0, 3),
            new("\u00F6", 0, 4),
            new("\u00FC", 0, 5)
        ];

        private static readonly Among[] A1 = [
            new("e", -1, 2),
            new("em", -1, 1),
            new("en", -1, 2),
            new("ern", -1, 1),
            new("er", -1, 1),
            new("s", -1, 3),
            new("es", 5, 2)
        ];

        private static readonly Among[] A2 = [
            new("en", -1, 1),
            new("er", -1, 1),
            new("st", -1, 2),
            new("est", 2, 1)
        ];

        private static readonly Among[] A3 = [
            new("ig", -1, 1),
            new("lich", -1, 1)
        ];

        private static readonly Among[] A4 = [
            new("end", -1, 1),
            new("ig", -1, 2),
            new("ung", -1, 1),
            new("lich", -1, 3),
            new("isch", -1, 2),
            new("ik", -1, 2),
            new("heit", -1, 3),
            new("keit", -1, 4)
        ];

        private static readonly int[] GV = [17, 65, 16, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 8, 0, 32, 8];

        private static readonly int[] GSEnding = [117, 30, 5];
        private static readonly int[] GStEnding = [117, 30, 4];

        private int IX;
        private int IP2;
        private int IP1;

        public void Stem()
        {
            int v1 = Cursor;
            RPrelude();
            Cursor = v1;
            RMarkRegions();
            LimitBackward = v1;
            Cursor = Limit;
            RStandardSuffix();
            Cursor = LimitBackward;
            RPostlude();
        }

        private void RPrelude()
        {
            int v1 = Cursor;
            int v2, v3, v4, v5;

            while(true)
            {
                v2 = Cursor;
                Bra = v2;
                if(EqS(1, "\u00DF"))
                {
                    Ket = Cursor;
                    SliceFrom("ss");
                }
                else
                {
                    if(v2 >= Limit)
                        break;
                    Cursor = v2 + 1;
                }
            }

            Cursor = v1;

            while(true)
            {
                v3 = Cursor;
                while(true)
                {
                    v4 = Cursor;
                    if(InGrouping(GV, 97, 252))
                    {
                        v5 = Cursor;
                        Bra = v5;
                        if(Habr1("u", "U", v4))
                            break;
                        Cursor = v5;
                        if(Habr1("y", "Y", v4))
                            break;
                    }
                    if(v4 >= Limit)
                    {
                        Cursor = v3;
                        return;
                    }
                    Cursor = v4 + 1;
                }
            }
        }

        private bool Habr1(string c1, string c2, int v1)
        {
            if(EqS(1, c1))
            {
                Ket = Cursor;
                if(InGrouping(GV, 97, 252))
                {
                    SliceFrom(c2);
                    Cursor = v1;
                    return true;
                }
            }
            return false;
        }

        private bool Habr2()
        {
            while(!InGrouping(GV, 97, 252))
            {
                if(Cursor >= Limit)
                    return true;
                Cursor++;
            }
            while(!OutGrouping(GV, 97, 252))
            {
                if(Cursor >= Limit)
                    return true;
                Cursor++;
            }
            return false;
        }

        private void RMarkRegions()
        {
            IP1 = Limit;
            IP2 = IP1;
            int c = Cursor + 3;
            if(0 <= c && c <= Limit)
            {
                IX = c;
                if(!Habr2())
                {
                    IP1 = Cursor;
                    if(IP1 < IX)
                        IP1 = IX;
                    if(!Habr2())
                        IP2 = Cursor;
                }
            }
        }

        private void RPostlude()
        {
            int amongVar, v1;
            while(true)
            {
                v1 = Cursor;
                Bra = v1;
                amongVar = FindAmong(A0, 6);
                if(amongVar == 0)
                    return;
                Ket = Cursor;
                switch(amongVar)
                {
                    case 1:
                        SliceFrom("y");
                        break;
                    case 2:
                    case 5:
                        SliceFrom("u");
                        break;
                    case 3:
                        SliceFrom("a");
                        break;
                    case 4:
                        SliceFrom("o");
                        break;
                    case 6:
                        if(Cursor >= Limit)
                            return;
                        Cursor++;
                        break;
                }
            }
        }

        private bool RR1()
        {
            return IP1 <= Cursor;
        }

        private bool RR2()
        {
            return IP2 <= Cursor;
        }

        private void RStandardSuffix()
        {
            int amongVar, v1 = Limit - Cursor;
            int v2, v3, v4;

            Ket = Cursor;
            amongVar = FindAmongB(A1, 7);
            if(amongVar != 0)
            {
                Bra = Cursor;
                if(RR1())
                {
                    switch(amongVar)
                    {
                        case 1:
                            SliceDel();
                            break;
                        case 2:
                            SliceDel();
                            Ket = Cursor;
                            if(EqSB(1, "s"))
                            {
                                Bra = Cursor;
                                if(EqSB(3, "nis"))
                                    SliceDel();
                            }
                            break;
                        case 3:
                            if(InGroupingB(GSEnding, 98, 116))
                                SliceDel();
                            break;
                    }
                }
            }

            Cursor = Limit - v1;
            Ket = Cursor;
            amongVar = FindAmongB(A2, 4);
            if(amongVar != 0)
            {
                Bra = Cursor;
                if(RR1())
                {
                    switch(amongVar)
                    {
                        case 1:
                            SliceDel();
                            break;
                        case 2:
                            if(InGroupingB(GStEnding, 98, 116))
                            {
                                int c = Cursor - 3;
                                if(LimitBackward <= c && c <= Limit)
                                {
                                    Cursor = c;
                                    SliceDel();
                                }
                            }
                            break;
                    }
                }
            }

            Cursor = Limit - v1;
            Ket = Cursor;
            amongVar = FindAmongB(A4, 8);
            if(amongVar != 0)
            {
                Bra = Cursor;
                if(RR2())
                {
                    switch(amongVar)
                    {
                        case 1:
                            SliceDel();
                            Ket = Cursor;
                            if(EqSB(2, "ig"))
                            {
                                Bra = Cursor;
                                v2 = Limit - Cursor;
                                if(!EqSB(1, "e"))
                                {
                                    Cursor = Limit - v2;
                                    if(RR2())
                                        SliceDel();
                                }
                            }
                            break;
                        case 2:
                            v3 = Limit - Cursor;
                            if(!EqSB(1, "e"))
                            {
                                Cursor = Limit - v3;
                                SliceDel();
                            }
                            break;
                        case 3:
                            SliceDel();
                            Ket = Cursor;
                            v4 = Limit - Cursor;
                            if(!EqSB(2, "er"))
                            {
                                Cursor = Limit - v4;
                                if(!EqSB(2, "en"))
                                    break;
                            }
                            Bra = Cursor;
                            if(RR1())
                                SliceDel();
                            break;
                        case 4:
                            SliceDel();
                            Ket = Cursor;
                            amongVar = FindAmongB(A3, 2);
                            if(amongVar != 0)
                            {
                                Bra = Cursor;
                                if(RR2() && amongVar == 1)
                                    SliceDel();
                            }
                            break;
                    }
                }
            }
        }
    }
}
