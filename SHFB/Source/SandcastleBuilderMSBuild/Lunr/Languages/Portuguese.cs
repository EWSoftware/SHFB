/*!
 * Lunr languages, `Portuguese` language
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
/// Portuguese language support for Lunr.
/// Provides stemming and stop word filtering for Portuguese text.
/// </summary>
public static class Portuguese
{
    /// <summary>
    /// Word characters specific to Portuguese language.
    /// </summary>
    public const string WordCharacters = "A-Za-z\u00AA\u00BA\u00C0-\u00D6\u00D8-\u00F6\u00F8-\u02B8\u02E0-\u02E4\u1D00-\u1D25\u1D2C-\u1D5C\u1D62-\u1D65\u1D6B-\u1D77\u1D79-\u1DBE\u1E00-\u1EFF\u2071\u207F\u2090-\u209C\u212A\u212B\u2132\u214E\u2160-\u2188\u2C60-\u2C7F\uA722-\uA787\uA78B-\uA7AD\uA7B0-\uA7B7\uA7F7-\uA7FF\uAB30-\uAB5A\uAB5C-\uAB64\uFB00-\uFB06\uFF21-\uFF3A\uFF41-\uFF5A";

    private static PipelineFunction _trimmer;
    private static PipelineFunction _stemmer;
    private static PipelineFunction _stopWordFilter;

    /// <summary>
    /// Gets the Portuguese trimmer pipeline function.
    /// </summary>
    public static PipelineFunction Trimmer
    {
        get
        {
            if(_trimmer == null)
            {
                _trimmer = TrimmerSupport.GenerateTrimmer(WordCharacters);
                Pipeline.RegisterFunction(_trimmer, "trimmer-pt");
            }
            return _trimmer;
        }
    }

    /// <summary>
    /// Gets the Portuguese stemmer pipeline function.
    /// </summary>
    public static PipelineFunction Stemmer
    {
        get
        {
            if(_stemmer == null)
            {
                _stemmer = CreateStemmer();
                Pipeline.RegisterFunction(_stemmer, "stemmer-pt");
            }
            return _stemmer;
        }
    }

    /// <summary>
    /// Gets the Portuguese stop word filter pipeline function.
    /// </summary>
    public static PipelineFunction StopWordFilter
    {
        get
        {
            if(_stopWordFilter == null)
            {
                _stopWordFilter = Lunr.StopWordFilter.GenerateStopWordFilter(StopWords);
                Pipeline.RegisterFunction(_stopWordFilter, "stopWordFilter-pt");
            }
            return _stopWordFilter;
        }
    }

    /// <summary>
    /// Portuguese stop words.
    /// </summary>
    public static readonly HashSet<string> StopWords =
    [
        "a", "ao", "aos", "aquela", "aquelas", "aquele", "aqueles", "aquilo", "as", "até",
        "com", "como", "da", "das", "de", "dela", "delas", "dele", "deles", "depois",
        "do", "dos", "e", "ela", "elas", "ele", "eles", "em", "entre", "era",
        "eram", "essa", "essas", "esse", "esses", "esta", "estamos", "estas", "estava", "estavam",
        "este", "esteja", "estejam", "estejamos", "estes", "esteve", "estive", "estivemos", "estiver", "estivera",
        "estiveram", "estiverem", "estivermos", "estivesse", "estivessem", "estivéramos", "estivéssemos", "estou", "está", "estávamos",
        "estão", "eu", "foi", "fomos", "for", "fora", "foram", "forem", "formos", "fosse",
        "fossem", "fui", "fôramos", "fôssemos", "haja", "hajam", "hajamos", "havemos", "hei", "houve",
        "houvemos", "houver", "houvera", "houveram", "houverem", "houveremos", "houveria", "houveriam", "houvermos", "houverá",
        "houverão", "houveríamos", "houvesse", "houvessem", "houvéramos", "houvéssemos", "há", "hão", "isso", "isto",
        "já", "lhe", "lhes", "mais", "mas", "me", "mesmo", "meu", "meus", "minha",
        "minhas", "muito", "na", "nas", "nem", "no", "nos", "nossa", "nossas", "nosso",
        "nossos", "num", "numa", "não", "nós", "o", "os", "ou", "para", "pela",
        "pelas", "pelo", "pelos", "por", "qual", "quando", "que", "quem", "se", "seja",
        "sejam", "sejamos", "sem", "serei", "seremos", "seria", "seriam", "será", "serão", "seríamos",
        "seu", "seus", "somos", "sou", "sua", "suas", "são", "só", "também", "te",
        "tem", "temos", "tenha", "tenham", "tenhamos", "tenho", "terei", "teremos", "teria", "teriam",
        "terá", "terão", "teríamos", "teu", "teus", "teve", "tinha", "tinham", "tive", "tivemos",
        "tiver", "tivera", "tiveram", "tiverem", "tivermos", "tivesse", "tivessem", "tivéramos", "tivéssemos", "tu",
        "tua", "tuas", "tém", "tínhamos", "um", "uma", "você", "vocês", "vos", "à",
        "às", "éramos"
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
                var stemmer = new PortugueseStemmer();
                stemmer.SetCurrent(word);
                stemmer.Stem();
                return stemmer.Current;
            });
        };
    }

    /// <summary>
    /// Configures a Builder to use Portuguese language processing.
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

    private sealed class PortugueseStemmer : SnowballProgram
    {
        // Portuguese stemmer uses similar approach to Spanish
        // The implementation is based on the Portuguese Snowball algorithm

        private static readonly Among[] A0 = [
            new("", -1, 3),
            new("\u00E3", 0, 1),
            new("\u00F5", 0, 2)
        ];

        private static readonly Among[] A1 = [
            new("", -1, 3),
            new("a~", 0, 1),
            new("o~", 0, 2)
        ];

        private static readonly Among[] A2 = [
            new("ic", -1, -1),
            new("ad", -1, -1),
            new("os", -1, -1),
            new("iv", -1, 1)
        ];

        private static readonly Among[] A3 = [
            new("ante", -1, 1),
            new("avel", -1, 1),
            new("\u00EDvel", -1, 1)
        ];

        private static readonly Among[] A4 = [
            new("ic", -1, 1),
            new("abil", -1, 1),
            new("iv", -1, 1)
        ];

        private static readonly Among[] A5 = [
            new("ica", -1, 1),
            new("\u00E2ncia", -1, 1),
            new("\u00EAncia", -1, 4),
            new("logia", -1, 2),
            new("ira", -1, 9),
            new("adora", -1, 1),
            new("osa", -1, 1),
            new("ista", -1, 1),
            new("iva", -1, 8),
            new("eza", -1, 1),
            new("idade", -1, 7),
            new("ante", -1, 1),
            new("mente", -1, 6),
            new("amente", 12, 5),
            new("\u00E1vel", -1, 1),
            new("\u00EDvel", -1, 1),
            new("ico", -1, 1),
            new("ismo", -1, 1),
            new("oso", -1, 1),
            new("amento", -1, 1),
            new("imento", -1, 1),
            new("ivo", -1, 8),
            new("a\u00E7a~o", -1, 1),
            new("u\u00E7a~o", -1, 3),
            new("ador", -1, 1),
            new("icas", -1, 1),
            new("\u00EAncias", -1, 4),
            new("logias", -1, 2),
            new("iras", -1, 9),
            new("adoras", -1, 1),
            new("osas", -1, 1),
            new("istas", -1, 1),
            new("ivas", -1, 8),
            new("ezas", -1, 1),
            new("idades", -1, 7),
            new("adores", -1, 1),
            new("antes", -1, 1),
            new("a\u00E7o~es", -1, 1),
            new("u\u00E7o~es", -1, 3),
            new("icos", -1, 1),
            new("ismos", -1, 1),
            new("osos", -1, 1),
            new("amentos", -1, 1),
            new("imentos", -1, 1),
            new("ivos", -1, 8)
        ];

        private static readonly Among[] A6 = [
            new("ada", -1, 1),
            new("ida", -1, 1),
            new("ia", -1, 1),
            new("aria", 2, 1),
            new("eria", 2, 1),
            new("iria", 2, 1),
            new("ara", -1, 1),
            new("era", -1, 1),
            new("ira", -1, 1),
            new("ava", -1, 1),
            new("asse", -1, 1),
            new("esse", -1, 1),
            new("isse", -1, 1),
            new("aste", -1, 1),
            new("este", -1, 1),
            new("iste", -1, 1),
            new("ei", -1, 1),
            new("arei", 16, 1),
            new("erei", 16, 1),
            new("irei", 16, 1),
            new("am", -1, 1),
            new("iam", 20, 1),
            new("aram", 20, 1),
            new("eram", 20, 1),
            new("iram", 20, 1),
            new("avam", 20, 1),
            new("em", -1, 1),
            new("arem", 26, 1),
            new("erem", 26, 1),
            new("irem", 26, 1),
            new("assem", 26, 1),
            new("essem", 26, 1),
            new("issem", 26, 1),
            new("ado", -1, 1),
            new("ido", -1, 1),
            new("ando", -1, 1),
            new("endo", -1, 1),
            new("indo", -1, 1),
            new("ara~o", -1, 1),
            new("era~o", -1, 1),
            new("ira~o", -1, 1),
            new("ar", -1, 1),
            new("er", -1, 1),
            new("ir", -1, 1),
            new("as", -1, 1),
            new("adas", 44, 1),
            new("idas", 44, 1),
            new("ias", 44, 1),
            new("arias", 47, 1),
            new("erias", 47, 1),
            new("irias", 47, 1),
            new("aras", 44, 1),
            new("eras", 44, 1),
            new("iras", 44, 1),
            new("avas", 44, 1),
            new("es", -1, 1),
            new("ardes", 55, 1),
            new("erdes", 55, 1),
            new("irdes", 55, 1),
            new("ares", 55, 1),
            new("eres", 55, 1),
            new("ires", 55, 1),
            new("asses", 55, 1),
            new("esses", 55, 1),
            new("isses", 55, 1),
            new("astes", 55, 1),
            new("estes", 55, 1),
            new("istes", 55, 1),
            new("is", -1, 1),
            new("ais", 68, 1),
            new("eis", 68, 1),
            new("areis", 70, 1),
            new("ereis", 70, 1),
            new("ireis", 70, 1),
            new("\u00E1reis", 70, 1),
            new("\u00E9reis", 70, 1),
            new("\u00EDreis", 70, 1),
            new("\u00E1sseis", 70, 1),
            new("\u00E9sseis", 70, 1),
            new("\u00EDsseis", 70, 1),
            new("\u00E1veis", 70, 1),
            new("\u00EDeis", 70, 1),
            new("ar\u00EDeis", 81, 1),
            new("er\u00EDeis", 81, 1),
            new("ir\u00EDeis", 81, 1),
            new("ados", -1, 1),
            new("idos", -1, 1),
            new("amos", -1, 1),
            new("\u00E1ramos", 87, 1),
            new("\u00E9ramos", 87, 1),
            new("\u00EDramos", 87, 1),
            new("\u00E1vamos", 87, 1),
            new("\u00EDamos", 87, 1),
            new("ar\u00EDamos", 92, 1),
            new("er\u00EDamos", 92, 1),
            new("ir\u00EDamos", 92, 1),
            new("emos", -1, 1),
            new("aremos", 96, 1),
            new("eremos", 96, 1),
            new("iremos", 96, 1),
            new("\u00E1ssemos", 96, 1),
            new("\u00EAssemos", 96, 1),
            new("\u00EDssemos", 96, 1),
            new("imos", -1, 1),
            new("armos", -1, 1),
            new("ermos", -1, 1),
            new("irmos", -1, 1),
            new("\u00E1mos", -1, 1),
            new("ar\u00E1s", -1, 1),
            new("er\u00E1s", -1, 1),
            new("ir\u00E1s", -1, 1),
            new("eu", -1, 1),
            new("iu", -1, 1),
            new("ou", -1, 1),
            new("ar\u00E1", -1, 1),
            new("er\u00E1", -1, 1),
            new("ir\u00E1", -1, 1)
        ];

        private static readonly Among[] A7 = [
            new("a", -1, 1),
            new("i", -1, 1),
            new("o", -1, 1),
            new("os", -1, 1),
            new("\u00E1", -1, 1),
            new("\u00ED", -1, 1),
            new("\u00F3", -1, 1)
        ];

        /*
        private static readonly Among[] A8 = [
            new("e", -1, 1),
            new("\u00E7", -1, 2),
            new("\u00E9", -1, 1),
            new("\u00EA", -1, 1)
        ];*/

        private static readonly int[] GV = [
            17, 65, 16, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 19, 12, 2
        ];

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
            RStandardSuffix();
            Cursor = Limit;
            RVerbSuffix();
            Cursor = Limit;
            RResidualSuffix();
            Cursor = LimitBackward;
            RPostlude();
        }

        private void RPrelude()
        {
            int v1;
            while(true)
            {
                v1 = Cursor;
                Bra = Cursor;
                if(FindAmong(A0, 3) != 0)
                {
                    Ket = Cursor;
                    if(Cursor < Limit)
                    {
                        Cursor++;
                        continue;
                    }
                }
                Cursor = v1;
                break;
            }
        }

        private void RMarkRegions()
        {
            int v1 = Cursor;
            IPV = Limit;
            IP1 = Limit;
            IP2 = Limit;

            if(InGrouping(GV, 97, 250))
            {
                if(InGrouping(GV, 97, 250))
                {
                    if(OutGrouping(GV, 97, 250))
                    {
                        while(true)
                        {
                            if(InGrouping(GV, 97, 250))
                                break;
                            if(Cursor >= Limit)
                            {
                                Cursor = v1;
                                goto lab2;
                            }
                            Cursor++;
                        }
                    }
                    IPV = Cursor;
                }
            }

            lab2:
            Cursor = v1;
            if(OutGrouping(GV, 97, 250))
            {
                while(true)
                {
                    if(InGrouping(GV, 97, 250))
                        break;
                    if(Cursor >= Limit)
                        return;
                    Cursor++;
                }
                while(true)
                {
                    if(OutGrouping(GV, 97, 250))
                        break;
                    if(Cursor >= Limit)
                        return;
                    Cursor++;
                }
                IP1 = Cursor;
                while(true)
                {
                    if(InGrouping(GV, 97, 250))
                        break;
                    if(Cursor >= Limit)
                        return;
                    Cursor++;
                }
                while(true)
                {
                    if(OutGrouping(GV, 97, 250))
                        break;
                    if(Cursor >= Limit)
                        return;
                    Cursor++;
                }
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
                        SliceFrom("\u00E3");
                        break;
                    case 2:
                        SliceFrom("\u00F5");
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

        private void RStandardSuffix()
        {
            int amongVar;
            Ket = Cursor;
            amongVar = FindAmongB(A5, 45);
            if(amongVar != 0)
            {
                Bra = Cursor;
                switch(amongVar)
                {
                    case 1:
                        if(RR2())
                            SliceDel();
                        break;
                    case 2:
                        if(RR2())
                            SliceFrom("log");
                        break;
                    case 3:
                        if(RR2())
                            SliceFrom("u");
                        break;
                    case 4:
                        if(RR2())
                            SliceFrom("ente");
                        break;
                    case 5:
                        if(RR1())
                        {
                            SliceDel();
                            Ket = Cursor;
                            amongVar = FindAmongB(A2, 4);
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
                        }
                        break;
                    case 6:
                        if(RR1())
                        {
                            SliceDel();
                            Ket = Cursor;
                            amongVar = FindAmongB(A3, 3);
                            if(amongVar != 0)
                            {
                                Bra = Cursor;
                                if(amongVar == 1)
                                    if(RR2())
                                        SliceDel();
                            }
                        }
                        break;
                    case 7:
                        if(RR2())
                        {
                            SliceDel();
                            Ket = Cursor;
                            amongVar = FindAmongB(A4, 3);
                            if(amongVar != 0)
                            {
                                Bra = Cursor;
                                if(amongVar == 1)
                                    if(RR2())
                                        SliceDel();
                            }
                        }
                        break;
                    case 8:
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
                    case 9:
                        if(RRV() && EqSB(1, "e"))
                            SliceDel();
                        break;
                }
            }
        }

        private void RVerbSuffix()
        {
            int amongVar;
            if(Cursor >= IPV)
            {
                int v1 = LimitBackward;
                LimitBackward = IPV;
                Ket = Cursor;
                amongVar = FindAmongB(A6, 120);
                if(amongVar != 0)
                {
                    Bra = Cursor;
                    if(amongVar == 1)
                        SliceDel();
                }
                LimitBackward = v1;
            }
        }

        private void RResidualSuffix()
        {
            int amongVar;
            Ket = Cursor;
            amongVar = FindAmongB(A7, 7);
            if(amongVar != 0)
            {
                Bra = Cursor;
                if(amongVar == 1)
                    if(RRV())
                        SliceDel();
            }
        }
    }
}
