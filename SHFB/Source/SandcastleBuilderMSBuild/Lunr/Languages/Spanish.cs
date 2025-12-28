/*!
 * Lunr languages, `Spanish` language
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
/// Spanish language support for Lunr.
/// Provides stemming and stop word filtering for Spanish text.
/// </summary>
public static class Spanish
{
    /// <summary>
    /// Word characters specific to Spanish language.
    /// </summary>
    public const string WordCharacters = "A-Za-z\u00AA\u00BA\u00C0-\u00D6\u00D8-\u00F6\u00F8-\u02B8\u02E0-\u02E4\u1D00-\u1D25\u1D2C-\u1D5C\u1D62-\u1D65\u1D6B-\u1D77\u1D79-\u1DBE\u1E00-\u1EFF\u2071\u207F\u2090-\u209C\u212A\u212B\u2132\u214E\u2160-\u2188\u2C60-\u2C7F\uA722-\uA787\uA78B-\uA7AD\uA7B0-\uA7B7\uA7F7-\uA7FF\uAB30-\uAB5A\uAB5C-\uAB64\uFB00-\uFB06\uFF21-\uFF3A\uFF41-\uFF5A";

    private static PipelineFunction _trimmer;
    private static PipelineFunction _stemmer;
    private static PipelineFunction _stopWordFilter;

    /// <summary>
    /// Gets the Spanish trimmer pipeline function.
    /// </summary>
    public static PipelineFunction Trimmer
    {
        get
        {
            if(_trimmer == null)
            {
                _trimmer = TrimmerSupport.GenerateTrimmer(WordCharacters);
                Pipeline.RegisterFunction(_trimmer, "trimmer-es");
            }
            return _trimmer;
        }
    }

    /// <summary>
    /// Gets the Spanish stemmer pipeline function.
    /// </summary>
    public static PipelineFunction Stemmer
    {
        get
        {
            if(_stemmer == null)
            {
                _stemmer = CreateStemmer();
                Pipeline.RegisterFunction(_stemmer, "stemmer-es");
            }
            return _stemmer;
        }
    }

    /// <summary>
    /// Gets the Spanish stop word filter pipeline function.
    /// </summary>
    public static PipelineFunction StopWordFilter
    {
        get
        {
            if(_stopWordFilter == null)
            {
                _stopWordFilter = Lunr.StopWordFilter.GenerateStopWordFilter(StopWords);
                Pipeline.RegisterFunction(_stopWordFilter, "stopWordFilter-es");
            }
            return _stopWordFilter;
        }
    }

    /// <summary>
    /// Spanish stop words.
    /// </summary>
    public static readonly HashSet<string> StopWords =
    [
        "a", "al", "algo", "algunas", "algunos", "ante", "antes", "como", "con", "contra",
        "cual", "cuando", "de", "del", "desde", "donde", "durante", "e", "el", "ella",
        "ellas", "ellos", "en", "entre", "era", "erais", "eran", "eras", "eres", "es",
        "esa", "esas", "ese", "eso", "esos", "esta", "estaba", "estabais", "estaban", "estabas",
        "estad", "estada", "estadas", "estado", "estados", "estamos", "estando", "estar", "estaremos", "estará",
        "estarán", "estarás", "estaré", "estaréis", "estaría", "estaríais", "estaríamos", "estarían", "estarías", "estas",
        "este", "estemos", "esto", "estos", "estoy", "estuve", "estuviera", "estuvierais", "estuvieran", "estuvieras",
        "estuvieron", "estuviese", "estuvieseis", "estuviesen", "estuvieses", "estuvimos", "estuviste", "estuvisteis", "estuviéramos", "estuviésemos",
        "estuvo", "está", "estábamos", "estáis", "están", "estás", "esté", "estéis", "estén", "estés",
        "fue", "fuera", "fuerais", "fueran", "fueras", "fueron", "fuese", "fueseis", "fuesen", "fueses",
        "fui", "fuimos", "fuiste", "fuisteis", "fuéramos", "fuésemos", "ha", "habida", "habidas", "habido",
        "habidos", "habiendo", "habremos", "habrá", "habrán", "habrás", "habré", "habréis", "habría", "habríais",
        "habríamos", "habrían", "habrías", "habéis", "había", "habíais", "habíamos", "habían", "habías", "han",
        "has", "hasta", "hay", "haya", "hayamos", "hayan", "hayas", "hayáis", "he", "hemos",
        "hube", "hubiera", "hubierais", "hubieran", "hubieras", "hubieron", "hubiese", "hubieseis", "hubiesen", "hubieses",
        "hubimos", "hubiste", "hubisteis", "hubiéramos", "hubiésemos", "hubo", "la", "las", "le", "les",
        "lo", "los", "me", "mi", "mis", "mucho", "muchos", "muy", "más", "mí",
        "mía", "mías", "mío", "míos", "nada", "ni", "no", "nos", "nosotras", "nosotros",
        "nuestra", "nuestras", "nuestro", "nuestros", "o", "os", "otra", "otras", "otro", "otros",
        "para", "pero", "poco", "por", "porque", "que", "quien", "quienes", "qué", "se",
        "sea", "seamos", "sean", "seas", "seremos", "será", "serán", "serás", "seré", "seréis",
        "sería", "seríais", "seríamos", "serían", "serías", "seáis", "sido", "siendo", "sin", "sobre",
        "sois", "somos", "son", "soy", "su", "sus", "suya", "suyas", "suyo", "suyos",
        "sí", "también", "tanto", "te", "tendremos", "tendrá", "tendrán", "tendrás", "tendré", "tendréis",
        "tendría", "tendríais", "tendríamos", "tendrían", "tendrías", "tened", "tenemos", "tenga", "tengamos", "tengan",
        "tengas", "tengo", "tengáis", "tenida", "tenidas", "tenido", "tenidos", "teniendo", "tenéis", "tenía",
        "teníais", "teníamos", "tenían", "tenías", "ti", "tiene", "tienen", "tienes", "todo", "todos",
        "tu", "tus", "tuve", "tuviera", "tuvierais", "tuvieran", "tuvieras", "tuvieron", "tuviese", "tuvieseis",
        "tuviesen", "tuvieses", "tuvimos", "tuviste", "tuvisteis", "tuviéramos", "tuviésemos", "tuvo", "tuya", "tuyas",
        "tuyo", "tuyos", "tú", "un", "una", "uno", "unos", "vosotras", "vosotros", "vuestra",
        "vuestras", "vuestro", "vuestros", "y", "ya", "yo", "él", "éramos"
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
                var stemmer = new SpanishStemmer();
                stemmer.SetCurrent(word);
                stemmer.Stem();
                return stemmer.Current;
            });
        };
    }

    /// <summary>
    /// Configures a Builder to use Spanish language processing.
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

    private sealed class SpanishStemmer : SnowballProgram
    {
        private static readonly Among[] A0 = [
            new("", -1, 6),
            new("\u00E1", 0, 1),
            new("\u00E9", 0, 2),
            new("\u00ED", 0, 3),
            new("\u00F3", 0, 4),
            new("\u00FA", 0, 5)
        ];

        private static readonly Among[] A1 = [
            new("la", -1, -1),
            new("sela", 0, -1),
            new("le", -1, -1),
            new("me", -1, -1),
            new("se", -1, -1),
            new("lo", -1, -1),
            new("selo", 5, -1),
            new("las", -1, -1),
            new("selas", 7, -1),
            new("les", -1, -1),
            new("los", -1, -1),
            new("selos", 10, -1),
            new("nos", -1, -1)
        ];

        private static readonly Among[] A2 = [
            new("ando", -1, 6),
            new("iendo", -1, 6),
            new("yendo", -1, 7),
            new("\u00E1ndo", -1, 2),
            new("i\u00E9ndo", -1, 1),
            new("ar", -1, 6),
            new("er", -1, 6),
            new("ir", -1, 6),
            new("\u00E1r", -1, 3),
            new("\u00E9r", -1, 4),
            new("\u00EDr", -1, 5)
        ];

        private static readonly Among[] A3 = [
            new("ic", -1, -1),
            new("ad", -1, -1),
            new("os", -1, -1),
            new("iv", -1, 1)
        ];

        private static readonly Among[] A4 = [
            new("able", -1, 1),
            new("ible", -1, 1),
            new("ante", -1, 1)
        ];

        private static readonly Among[] A5 = [
            new("ic", -1, 1),
            new("abil", -1, 1),
            new("iv", -1, 1)
        ];

        private static readonly Among[] A6 = [
            new("ica", -1, 1),
            new("ancia", -1, 2),
            new("encia", -1, 5),
            new("adora", -1, 2),
            new("osa", -1, 1),
            new("ista", -1, 1),
            new("iva", -1, 9),
            new("anza", -1, 1),
            new("log\u00EDa", -1, 3),
            new("idad", -1, 8),
            new("able", -1, 1),
            new("ible", -1, 1),
            new("ante", -1, 2),
            new("mente", -1, 7),
            new("amente", 13, 6),
            new("aci\u00F3n", -1, 2),
            new("uci\u00F3n", -1, 4),
            new("ico", -1, 1),
            new("ismo", -1, 1),
            new("oso", -1, 1),
            new("amiento", -1, 1),
            new("imiento", -1, 1),
            new("ivo", -1, 9),
            new("ador", -1, 2),
            new("icas", -1, 1),
            new("ancias", -1, 2),
            new("encias", -1, 5),
            new("adoras", -1, 2),
            new("osas", -1, 1),
            new("istas", -1, 1),
            new("ivas", -1, 9),
            new("anzas", -1, 1),
            new("log\u00EDas", -1, 3),
            new("idades", -1, 8),
            new("ables", -1, 1),
            new("ibles", -1, 1),
            new("aciones", -1, 2),
            new("uciones", -1, 4),
            new("adores", -1, 2),
            new("antes", -1, 2),
            new("icos", -1, 1),
            new("ismos", -1, 1),
            new("osos", -1, 1),
            new("amientos", -1, 1),
            new("imientos", -1, 1),
            new("ivos", -1, 9)
        ];

        private static readonly Among[] A7 = [
            new("ya", -1, 1),
            new("ye", -1, 1),
            new("yan", -1, 1),
            new("yen", -1, 1),
            new("yeron", -1, 1),
            new("yendo", -1, 1),
            new("yo", -1, 1),
            new("yas", -1, 1),
            new("yes", -1, 1),
            new("yais", -1, 1),
            new("yamos", -1, 1),
            new("y\u00F3", -1, 1)
        ];

        private static readonly Among[] A8 = [
            new("aba", -1, 2),
            new("ada", -1, 2),
            new("ida", -1, 2),
            new("ara", -1, 2),
            new("iera", -1, 2),
            new("\u00EDa", -1, 2),
            new("ar\u00EDa", 5, 2),
            new("er\u00EDa", 5, 2),
            new("ir\u00EDa", 5, 2),
            new("ad", -1, 2),
            new("ed", -1, 2),
            new("id", -1, 2),
            new("ase", -1, 2),
            new("iese", -1, 2),
            new("aste", -1, 2),
            new("iste", -1, 2),
            new("an", -1, 2),
            new("aban", 16, 2),
            new("aran", 16, 2),
            new("ieran", 16, 2),
            new("\u00EDan", 16, 2),
            new("ar\u00EDan", 20, 2),
            new("er\u00EDan", 20, 2),
            new("ir\u00EDan", 20, 2),
            new("en", -1, 1),
            new("asen", 24, 2),
            new("iesen", 24, 2),
            new("aron", -1, 2),
            new("ieron", -1, 2),
            new("ar\u00E1n", -1, 2),
            new("er\u00E1n", -1, 2),
            new("ir\u00E1n", -1, 2),
            new("ado", -1, 2),
            new("ido", -1, 2),
            new("ando", -1, 2),
            new("iendo", -1, 2),
            new("ar", -1, 2),
            new("er", -1, 2),
            new("ir", -1, 2),
            new("as", -1, 2),
            new("abas", 39, 2),
            new("adas", 39, 2),
            new("idas", 39, 2),
            new("aras", 39, 2),
            new("ieras", 39, 2),
            new("\u00EDas", 39, 2),
            new("ar\u00EDas", 45, 2),
            new("er\u00EDas", 45, 2),
            new("ir\u00EDas", 45, 2),
            new("es", -1, 1),
            new("ases", 49, 2),
            new("ieses", 49, 2),
            new("abais", -1, 2),
            new("arais", -1, 2),
            new("ierais", -1, 2),
            new("\u00EDais", -1, 2),
            new("ar\u00EDais", 55, 2),
            new("er\u00EDais", 55, 2),
            new("ir\u00EDais", 55, 2),
            new("aseis", -1, 2),
            new("ieseis", -1, 2),
            new("asteis", -1, 2),
            new("isteis", -1, 2),
            new("\u00E1is", -1, 2),
            new("\u00E9is", -1, 1),
            new("ar\u00E9is", 64, 2),
            new("er\u00E9is", 64, 2),
            new("ir\u00E9is", 64, 2),
            new("ados", -1, 2),
            new("idos", -1, 2),
            new("amos", -1, 2),
            new("\u00E1bamos", 70, 2),
            new("\u00E1ramos", 70, 2),
            new("i\u00E9ramos", 70, 2),
            new("\u00EDamos", 70, 2),
            new("ar\u00EDamos", 74, 2),
            new("er\u00EDamos", 74, 2),
            new("ir\u00EDamos", 74, 2),
            new("emos", -1, 1),
            new("aremos", 78, 2),
            new("eremos", 78, 2),
            new("iremos", 78, 2),
            new("\u00E1semos", 78, 2),
            new("i\u00E9semos", 78, 2),
            new("imos", -1, 2),
            new("ar\u00E1s", -1, 2),
            new("er\u00E1s", -1, 2),
            new("ir\u00E1s", -1, 2),
            new("\u00EDs", -1, 2),
            new("ar\u00E1", -1, 2),
            new("er\u00E1", -1, 2),
            new("ir\u00E1", -1, 2),
            new("ar\u00E9", -1, 2),
            new("er\u00E9", -1, 2),
            new("ir\u00E9", -1, 2),
            new("i\u00F3", -1, 2)
        ];

        private static readonly Among[] A9 = [
            new("a", -1, 1),
            new("e", -1, 2),
            new("o", -1, 1),
            new("os", -1, 1),
            new("\u00E1", -1, 1),
            new("\u00E9", -1, 2),
            new("\u00ED", -1, 1),
            new("\u00F3", -1, 1)
        ];

        private static readonly int[] GV = [17, 65, 16, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 17, 4, 10];

        private int IPV;
        private int IP1;
        private int IP2;

        public void Stem()
        {
            int v1 = Cursor;
            RMarkRegions();
            LimitBackward = v1;
            Cursor = Limit;
            RAttachedPronoun();
            Cursor = Limit;
            if(!RStandardSuffix())
            {
                Cursor = Limit;
                if(!RYVerbSuffix())
                {
                    Cursor = Limit;
                    RVerbSuffix();
                }
            }
            Cursor = Limit;
            RResidualSuffix();
            Cursor = LimitBackward;
            RPostlude();
        }

        private bool Habr1()
        {
            if(OutGrouping(GV, 97, 252))
            {
                while(!InGrouping(GV, 97, 252))
                {
                    if(Cursor >= Limit)
                        return true;
                    Cursor++;
                }
                return false;
            }
            return true;
        }

        private bool Habr2()
        {
            if(InGrouping(GV, 97, 252))
            {
                int v1 = Cursor;
                if(Habr1())
                {
                    Cursor = v1;
                    if(!InGrouping(GV, 97, 252))
                        return true;
                    while(!OutGrouping(GV, 97, 252))
                    {
                        if(Cursor >= Limit)
                            return true;
                        Cursor++;
                    }
                }
                return false;
            }
            return true;
        }

        private void Habr3()
        {
            int v1 = Cursor;
            if(Habr2())
            {
                Cursor = v1;
                if(!OutGrouping(GV, 97, 252))
                    return;
                int v2 = Cursor;
                if(Habr1())
                {
                    Cursor = v2;
                    if(!InGrouping(GV, 97, 252) || Cursor >= Limit)
                        return;
                    Cursor++;
                }
            }
            IPV = Cursor;
        }

        private bool Habr4()
        {
            while(!InGrouping(GV, 97, 252))
            {
                if(Cursor >= Limit)
                    return false;
                Cursor++;
            }
            while(!OutGrouping(GV, 97, 252))
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
            Habr3();
            Cursor = v1;
            if(Habr4())
            {
                IP1 = Cursor;
                if(Habr4())
                    IP2 = Cursor;
            }
        }

        private void RPostlude()
        {
            int amongVar;
            while(true)
            {
                Bra = Cursor;
                amongVar = FindAmong(A0, 6);
                if(amongVar != 0)
                {
                    Ket = Cursor;
                    switch(amongVar)
                    {
                        case 1:
                            SliceFrom("a");
                            continue;
                        case 2:
                            SliceFrom("e");
                            continue;
                        case 3:
                            SliceFrom("i");
                            continue;
                        case 4:
                            SliceFrom("o");
                            continue;
                        case 5:
                            SliceFrom("u");
                            continue;
                        case 6:
                            if(Cursor >= Limit)
                                break;
                            Cursor++;
                            continue;
                    }
                }
                break;
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
            if(FindAmongB(A1, 13) != 0)
            {
                Bra = Cursor;
                amongVar = FindAmongB(A2, 11);
                if(amongVar != 0 && RRV())
                {
                    switch(amongVar)
                    {
                        case 1:
                            Bra = Cursor;
                            SliceFrom("iendo");
                            break;
                        case 2:
                            Bra = Cursor;
                            SliceFrom("ando");
                            break;
                        case 3:
                            Bra = Cursor;
                            SliceFrom("ar");
                            break;
                        case 4:
                            Bra = Cursor;
                            SliceFrom("er");
                            break;
                        case 5:
                            Bra = Cursor;
                            SliceFrom("ir");
                            break;
                        case 6:
                            SliceDel();
                            break;
                        case 7:
                            if(EqSB(1, "u"))
                                SliceDel();
                            break;
                    }
                }
            }
        }

        private bool Habr5(Among[] a, int n)
        {
            if(!RR2())
                return true;
            SliceDel();
            Ket = Cursor;
            int amongVar = FindAmongB(a, n);
            if(amongVar != 0)
            {
                Bra = Cursor;
                if(amongVar == 1 && RR2())
                    SliceDel();
            }
            return false;
        }

        private bool Habr6(string c1)
        {
            if(!RR2())
                return true;
            SliceDel();
            Ket = Cursor;
            if(EqSB(2, c1))
            {
                Bra = Cursor;
                if(RR2())
                    SliceDel();
            }
            return false;
        }

        private bool RStandardSuffix()
        {
            int amongVar;
            Ket = Cursor;
            amongVar = FindAmongB(A6, 46);
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
                        if(Habr6("ic"))
                            return false;
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
                        if(!RR1())
                            return false;
                        SliceDel();
                        Ket = Cursor;
                        amongVar = FindAmongB(A3, 4);
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
                    case 7:
                        if(Habr5(A4, 3))
                            return false;
                        break;
                    case 8:
                        if(Habr5(A5, 3))
                            return false;
                        break;
                    case 9:
                        if(Habr6("at"))
                            return false;
                        break;
                }
                return true;
            }
            return false;
        }

        private bool RYVerbSuffix()
        {
            int amongVar, v1;
            if(Cursor >= IPV)
            {
                v1 = LimitBackward;
                LimitBackward = IPV;
                Ket = Cursor;
                amongVar = FindAmongB(A7, 12);
                LimitBackward = v1;
                if(amongVar != 0)
                {
                    Bra = Cursor;
                    if(amongVar == 1)
                    {
                        if(!EqSB(1, "u"))
                            return false;
                        SliceDel();
                    }
                    return true;
                }
            }
            return false;
        }

        private void RVerbSuffix()
        {
            int amongVar, v1, v2, v3;
            if(Cursor >= IPV)
            {
                v1 = LimitBackward;
                LimitBackward = IPV;
                Ket = Cursor;
                amongVar = FindAmongB(A8, 96);
                LimitBackward = v1;
                if(amongVar != 0)
                {
                    Bra = Cursor;
                    switch(amongVar)
                    {
                        case 1:
                            v2 = Limit - Cursor;
                            if(EqSB(1, "u"))
                            {
                                v3 = Limit - Cursor;
                                if(EqSB(1, "g"))
                                    Cursor = Limit - v3;
                                else
                                    Cursor = Limit - v2;
                            }
                            else
                                Cursor = Limit - v2;
                            Bra = Cursor;
                            goto case 2;
                        case 2:
                            SliceDel();
                            break;
                    }
                }
            }
        }

        private void RResidualSuffix()
        {
            int amongVar, v1;
            Ket = Cursor;
            amongVar = FindAmongB(A9, 8);
            if(amongVar != 0)
            {
                Bra = Cursor;
                switch(amongVar)
                {
                    case 1:
                        if(RRV())
                            SliceDel();
                        break;
                    case 2:
                        if(RRV())
                        {
                            SliceDel();
                            Ket = Cursor;
                            if(EqSB(1, "u"))
                            {
                                Bra = Cursor;
                                v1 = Limit - Cursor;
                                if(EqSB(1, "g"))
                                {
                                    Cursor = Limit - v1;
                                    if(RRV())
                                        SliceDel();
                                }
                            }
                        }
                        break;
                }
            }
        }
    }
}
