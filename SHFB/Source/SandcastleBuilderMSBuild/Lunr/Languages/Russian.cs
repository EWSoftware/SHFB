/*!
 * Lunr languages, `Russian` language
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
/// Russian language support for Lunr.
/// Provides stemming and stop word filtering for Russian text.
/// </summary>
public static class Russian
{
    /// <summary>
    /// Word characters specific to Russian language (Cyrillic).
    /// </summary>
    public const string WordCharacters = "\u0400-\u0484\u0487-\u052F\u1D2B\u1D78\u2DE0-\u2DFF\uA640-\uA69F\uFE2E\uFE2F";

    private static PipelineFunction _trimmer;
    private static PipelineFunction _stemmer;
    private static PipelineFunction _stopWordFilter;

    /// <summary>
    /// Gets the Russian trimmer pipeline function.
    /// </summary>
    public static PipelineFunction Trimmer
    {
        get
        {
            if(_trimmer == null)
            {
                _trimmer = TrimmerSupport.GenerateTrimmer(WordCharacters);
                Pipeline.RegisterFunction(_trimmer, "trimmer-ru");
            }
            return _trimmer;
        }
    }

    /// <summary>
    /// Gets the Russian stemmer pipeline function.
    /// </summary>
    public static PipelineFunction Stemmer
    {
        get
        {
            if(_stemmer == null)
            {
                _stemmer = CreateStemmer();
                Pipeline.RegisterFunction(_stemmer, "stemmer-ru");
            }
            return _stemmer;
        }
    }

    /// <summary>
    /// Gets the Russian stop word filter pipeline function.
    /// </summary>
    public static PipelineFunction StopWordFilter
    {
        get
        {
            if(_stopWordFilter == null)
            {
                _stopWordFilter = Lunr.StopWordFilter.GenerateStopWordFilter(StopWords);
                Pipeline.RegisterFunction(_stopWordFilter, "stopWordFilter-ru");
            }
            return _stopWordFilter;
        }
    }

    /// <summary>
    /// Russian stop words.
    /// </summary>
    public static readonly HashSet<string> StopWords =
    [
        "алло", "без", "близко", "более", "больше", "будем", "будет", "будете", "будешь", "будто", "буду",
        "будут", "будь", "бы", "бывает", "бывь", "был", "была", "были", "было", "быть", "в", "важная", "важное",
        "важные", "важный", "вам", "вами", "вас", "ваш", "ваша", "ваше", "ваши", "вверх", "вдали", "вдруг",
        "ведь", "везде", "весь", "вниз", "внизу", "во", "вокруг", "вон", "восемнадцатый", "восемнадцать",
        "восемь", "восьмой", "вот", "впрочем", "времени", "время", "все", "всегда", "всего", "всем", "всеми",
        "всему", "всех", "всею", "всю", "всюду", "вся", "всё", "второй", "вы", "г", "где", "говорил", "говорит",
        "год", "года", "году", "да", "давно", "даже", "далеко", "дальше", "даром", "два", "двадцатый",
        "двадцать", "две", "двенадцатый", "двенадцать", "двух", "девятнадцатый", "девятнадцать", "девятый",
        "девять", "действительно", "дел", "день", "десятый", "десять", "для", "до", "довольно", "долго",
        "должно", "другая", "другие", "других", "друго", "другое", "другой", "е", "его", "ее", "ей", "ему",
        "если", "есть", "еще", "ещё", "ею", "её", "ж", "же", "жизнь", "за", "занят", "занята", "занято",
        "заняты", "затем", "зато", "зачем", "здесь", "значит", "и", "из", "или", "им", "именно", "иметь", "ими",
        "имя", "иногда", "их", "к", "каждая", "каждое", "каждые", "каждый", "кажется", "как", "какая", "какой",
        "кем", "когда", "кого", "ком", "кому", "конечно", "которая", "которого", "которой", "которые", "который",
        "которых", "кроме", "кругом", "кто", "куда", "лет", "ли", "лишь", "лучше", "люди", "м", "мало", "между",
        "меля", "менее", "меньше", "меня", "миллионов", "мимо", "мира", "мне", "много", "многочисленная",
        "многочисленное", "многочисленные", "многочисленный", "мной", "мною", "мог", "могут", "мож", "может",
        "можно", "можхо", "мои", "мой", "мор", "мочь", "моя", "моё", "мы", "на", "наверху", "над", "надо",
        "назад", "наиболее", "наконец", "нам", "нами", "нас", "начала", "наш", "наша", "наше", "наши", "не",
        "него", "недавно", "недалеко", "нее", "ней", "нельзя", "нем", "немного", "нему", "непрерывно", "нередко",
        "несколько", "нет", "нею", "неё", "ни", "нибудь", "ниже", "низко", "никогда", "никуда", "ними", "них",
        "ничего", "но", "ну", "нужно", "нх", "о", "об", "оба", "обычно", "один", "одиннадцатый", "одиннадцать",
        "однажды", "однако", "одного", "одной", "около", "он", "она", "они", "оно", "опять", "особенно", "от",
        "отовсюду", "отсюда", "очень", "первый", "перед", "по", "под", "пожалуйста", "позже", "пока", "пор",
        "пора", "после", "посреди", "потом", "потому", "почему", "почти", "прекрасно", "при", "про", "просто",
        "против", "процентов", "пятнадцатый", "пятнадцать", "пятый", "пять", "раз", "разве", "рано", "раньше",
        "рядом", "с", "сам", "сама", "сами", "самим", "самими", "самих", "само", "самого", "самой", "самом",
        "самому", "саму", "свое", "своего", "своей", "свои", "своих", "свою", "сеаой", "себе", "себя", "сегодня",
        "седьмой", "сейчас", "семнадцатый", "семнадцать", "семь", "сих", "сказал", "сказала", "сказать",
        "сколько", "слишком", "сначала", "снова", "со", "собой", "собою", "совсем", "спасибо", "стал", "суть",
        "т", "та", "так", "такая", "также", "такие", "такое", "такой", "там", "твой", "твоя", "твоё", "те",
        "тебе", "тебя", "тем", "теми", "теперь", "тех", "то", "тобой", "тобою", "тогда", "того", "тоже",
        "только", "том", "тому", "тот", "тою", "третий", "три", "тринадцатый", "тринадцать", "ту", "туда", "тут",
        "ты", "тысяч", "у", "уж", "уже", "уметь", "хорошо", "хотеть", "хоть", "хотя", "хочешь", "часто", "чаще",
        "чего", "человек", "чем", "чему", "через", "четвертый", "четыре", "четырнадцатый", "четырнадцать", "что",
        "чтоб", "чтобы", "чуть", "шестнадцатый", "шестнадцать", "шестой", "шесть", "эта", "эти", "этим", "этими",
        "этих", "это", "этого", "этой", "этом", "этому", "этот", "эту", "я", "﻿а"
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
                var stemmer = new RussianStemmer();
                stemmer.SetCurrent(word);
                stemmer.Stem();
                return stemmer.Current;
            });
        };
    }

    /// <summary>
    /// Configures a Builder to use Russian language processing.
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

    private sealed class RussianStemmer : SnowballProgram
    {
        // Perfective gerund patterns
        private static readonly Among[] A0 = [
            new("\u0432", -1, 1),
            new("\u0438\u0432", 0, 2),
            new("\u044B\u0432", 0, 2),
            new("\u0432\u0448\u0438", -1, 1),
            new("\u0438\u0432\u0448\u0438", 3, 2),
            new("\u044B\u0432\u0448\u0438", 3, 2),
            new("\u0432\u0448\u0438\u0441\u044C", -1, 1),
            new("\u0438\u0432\u0448\u0438\u0441\u044C", 6, 2),
            new("\u044B\u0432\u0448\u0438\u0441\u044C", 6, 2)
        ];

        // Adjective patterns
        private static readonly Among[] A1 = [
            new("\u0435\u0435", -1, 1),
            new("\u0438\u0435", -1, 1),
            new("\u043E\u0435", -1, 1),
            new("\u044B\u0435", -1, 1),
            new("\u0438\u043C\u0438", -1, 1),
            new("\u044B\u043C\u0438", -1, 1),
            new("\u0435\u0439", -1, 1),
            new("\u0438\u0439", -1, 1),
            new("\u043E\u0439", -1, 1),
            new("\u044B\u0439", -1, 1),
            new("\u0435\u043C", -1, 1),
            new("\u0438\u043C", -1, 1),
            new("\u043E\u043C", -1, 1),
            new("\u044B\u043C", -1, 1),
            new("\u0435\u0433\u043E", -1, 1),
            new("\u043E\u0433\u043E", -1, 1),
            new("\u0435\u043C\u0443", -1, 1),
            new("\u043E\u043C\u0443", -1, 1),
            new("\u0438\u0445", -1, 1),
            new("\u044B\u0445", -1, 1),
            new("\u0435\u044E", -1, 1),
            new("\u043E\u044E", -1, 1),
            new("\u0443\u044E", -1, 1),
            new("\u044E\u044E", -1, 1),
            new("\u0430\u044F", -1, 1),
            new("\u044F\u044F", -1, 1)
        ];

        // Participle patterns
        private static readonly Among[] A2 = [
            new("\u0435\u043C", -1, 1),
            new("\u043D\u043D", -1, 1),
            new("\u0432\u0448", -1, 1),
            new("\u0438\u0432\u0448", 2, 2),
            new("\u044B\u0432\u0448", 2, 2),
            new("\u0449", -1, 1),
            new("\u044E\u0449", 5, 1),
            new("\u0443\u044E\u0449", 6, 2)
        ];

        // Reflexive patterns
        private static readonly Among[] A3 = [
            new("\u0441\u044C", -1, 1),
            new("\u0441\u044F", -1, 1)
        ];

        // Verb patterns
        private static readonly Among[] A4 = [
            new("\u043B\u0430", -1, 1),
            new("\u0438\u043B\u0430", 0, 2),
            new("\u044B\u043B\u0430", 0, 2),
            new("\u043D\u0430", -1, 1),
            new("\u0435\u043D\u0430", 3, 2),
            new("\u0435\u0442\u0435", -1, 1),
            new("\u0438\u0442\u0435", -1, 2),
            new("\u0439\u0442\u0435", -1, 1),
            new("\u0435\u0439\u0442\u0435", 7, 2),
            new("\u0443\u0439\u0442\u0435", 7, 2),
            new("\u043B\u0438", -1, 1),
            new("\u0438\u043B\u0438", 10, 2),
            new("\u044B\u043B\u0438", 10, 2),
            new("\u0439", -1, 1),
            new("\u0435\u0439", 13, 2),
            new("\u0443\u0439", 13, 2),
            new("\u043B", -1, 1),
            new("\u0438\u043B", 16, 2),
            new("\u044B\u043B", 16, 2),
            new("\u0435\u043C", -1, 1),
            new("\u0438\u043C", -1, 2),
            new("\u044B\u043C", -1, 2),
            new("\u043D", -1, 1),
            new("\u0435\u043D", 22, 2),
            new("\u043B\u043E", -1, 1),
            new("\u0438\u043B\u043E", 24, 2),
            new("\u044B\u043B\u043E", 24, 2),
            new("\u043D\u043E", -1, 1),
            new("\u0435\u043D\u043E", 27, 2),
            new("\u043D\u043D\u043E", 27, 1),
            new("\u0435\u0442", -1, 1),
            new("\u0443\u0435\u0442", 30, 2),
            new("\u0438\u0442", -1, 2),
            new("\u044B\u0442", -1, 2),
            new("\u044E\u0442", -1, 1),
            new("\u0443\u044E\u0442", 34, 2),
            new("\u044F\u0442", -1, 2),
            new("\u043D\u044B", -1, 1),
            new("\u0435\u043D\u044B", 37, 2),
            new("\u0442\u044C", -1, 1),
            new("\u0438\u0442\u044C", 39, 2),
            new("\u044B\u0442\u044C", 39, 2),
            new("\u0435\u0448\u044C", -1, 1),
            new("\u0438\u0448\u044C", -1, 2),
            new("\u044E", -1, 2),
            new("\u0443\u044E", 44, 2)
        ];

        // Noun patterns
        private static readonly Among[] A5 = [
            new("\u0430", -1, 1),
            new("\u0435\u0432", -1, 1),
            new("\u043E\u0432", -1, 1),
            new("\u0435", -1, 1),
            new("\u0438\u0435", 3, 1),
            new("\u044C\u0435", 3, 1),
            new("\u0438", -1, 1),
            new("\u0435\u0438", 6, 1),
            new("\u0438\u0438", 6, 1),
            new("\u0430\u043C\u0438", 6, 1),
            new("\u044F\u043C\u0438", 6, 1),
            new("\u0438\u044F\u043C\u0438", 10, 1),
            new("\u0439", -1, 1),
            new("\u0435\u0439", 12, 1),
            new("\u0438\u0435\u0439", 13, 1),
            new("\u0438\u0439", 12, 1),
            new("\u043E\u0439", 12, 1),
            new("\u0430\u043C", -1, 1),
            new("\u0435\u043C", -1, 1),
            new("\u0438\u0435\u043C", 18, 1),
            new("\u043E\u043C", -1, 1),
            new("\u044F\u043C", -1, 1),
            new("\u0438\u044F\u043C", 21, 1),
            new("\u043E", -1, 1),
            new("\u0443", -1, 1),
            new("\u0430\u0445", -1, 1),
            new("\u044F\u0445", -1, 1),
            new("\u0438\u044F\u0445", 26, 1),
            new("\u044B", -1, 1),
            new("\u044C", -1, 1),
            new("\u044E", -1, 1),
            new("\u0438\u044E", 30, 1),
            new("\u044C\u044E", 30, 1),
            new("\u044F", -1, 1),
            new("\u0438\u044F", 33, 1),
            new("\u044C\u044F", 33, 1)
        ];

        // Derivational patterns
        private static readonly Among[] A6 = [
            new("\u043E\u0441\u0442", -1, 1),
            new("\u043E\u0441\u0442\u044C", -1, 1)
        ];

        // Superlative patterns
        private static readonly Among[] A7 = [
            new("\u0435\u0439\u0448\u0435", -1, 1),
            new("\u043D", -1, 2),
            new("\u0435\u0439\u0448", -1, 1),
            new("\u044C", -1, 3)
        ];

        // Vowel grouping for Russian (Cyrillic vowels ?, ?, ?, ?, ?, ?, ?, ?, ?)
        private static readonly int[] GV = [33, 65, 8, 232];

        private int IPV;
        private int IP2;

        public void Stem()
        {
            RMarkRegions();
            Cursor = Limit;
            if(Cursor < IPV)
                return;
            LimitBackward = IPV;

            if(!RPerfectiveGerund())
            {
                Cursor = Limit;
                if(!RReflexive())
                    Cursor = Limit;

                if(!RAdjectival())
                {
                    Cursor = Limit;
                    if(!RVerb())
                    {
                        Cursor = Limit;
                        RNoun();
                    }
                }
            }

            Cursor = Limit;
            Ket = Cursor;
            if(EqSB(1, "\u0438"))
            {
                Bra = Cursor;
                SliceDel();
            }
            else
                Cursor = Limit;

            RDerivational();
            Cursor = Limit;
            RTidyUp();
        }

        private bool Habr3()
        {
            while(!InGrouping(GV, 1072, 1103))
            {
                if(Cursor >= Limit)
                    return false;
                Cursor++;
            }
            return true;
        }

        private bool Habr4()
        {
            while(!OutGrouping(GV, 1072, 1103))
            {
                if(Cursor >= Limit)
                    return false;
                Cursor++;
            }
            return true;
        }

        private void RMarkRegions()
        {
            IPV = Limit;
            IP2 = IPV;

            if(Habr3())
            {
                IPV = Cursor;
                if(Habr4())
                    if(Habr3())
                        if(Habr4())
                            IP2 = Cursor;
            }
        }

        private bool RR2()
        {
            return IP2 <= Cursor;
        }

        private bool Habr2(Among[] a, int n)
        {
            int amongVar, v1;
            Ket = Cursor;
            amongVar = FindAmongB(a, n);
            if(amongVar != 0)
            {
                Bra = Cursor;
                switch(amongVar)
                {
                    case 1:
                        v1 = Limit - Cursor;
                        if(!EqSB(1, "\u0430"))
                        {
                            Cursor = Limit - v1;
                            if(!EqSB(1, "\u044F"))
                                return false;
                        }
                        goto case 2;
                    case 2:
                        SliceDel();
                        break;
                }
                return true;
            }
            return false;
        }

        private bool RPerfectiveGerund()
        {
            return Habr2(A0, 9);
        }

        private bool Habr1(Among[] a, int n)
        {
            int amongVar;
            Ket = Cursor;
            amongVar = FindAmongB(a, n);
            if(amongVar != 0)
            {
                Bra = Cursor;
                if(amongVar == 1)
                    SliceDel();
                return true;
            }
            return false;
        }

        private bool RAdjective()
        {
            return Habr1(A1, 26);
        }

        private bool RAdjectival()
        {
            if(RAdjective())
            {
                Habr2(A2, 8);
                return true;
            }
            return false;
        }

        private bool RReflexive()
        {
            return Habr1(A3, 2);
        }

        private bool RVerb()
        {
            return Habr2(A4, 46);
        }

        private void RNoun()
        {
            Habr1(A5, 36);
        }

        private void RDerivational()
        {
            int amongVar;
            Ket = Cursor;
            amongVar = FindAmongB(A6, 2);
            if(amongVar != 0)
            {
                Bra = Cursor;
                if(RR2() && amongVar == 1)
                    SliceDel();
            }
        }

        private void RTidyUp()
        {
            int amongVar;
            Ket = Cursor;
            amongVar = FindAmongB(A7, 4);
            if(amongVar != 0)
            {
                Bra = Cursor;
                switch(amongVar)
                {
                    case 1:
                        SliceDel();
                        Ket = Cursor;
                        if(!EqSB(1, "\u043D"))
                            break;
                        Bra = Cursor;
                        goto case 2;
                    case 2:
                        if(!EqSB(1, "\u043D"))
                            break;
                        goto case 3;
                    case 3:
                        SliceDel();
                        break;
                }
            }
        }
    }
}
