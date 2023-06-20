using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace TestDoc.Generics.HardToMatchSignatures
{
    /// <summary>Class A.</summary>
    /// <typeparam name="T">T</typeparam>
    public class A<T>
    {
        /// <summary>Method M, TUT variant.</summary>
        /// <param name="f">f</param>
        /// <param name="x">x</param>
        /// <typeparam name="U">U</typeparam>
        /// <returns>Integer</returns>
        public int M<U>(Func<T, U, T> f, int x) => x;

        /// <summary>Method M, UTT variant.</summary>
        /// <param name="f">f</param>
        /// <param name="x">x</param>
        /// <typeparam name="U">U</typeparam>
        /// <returns>Integer</returns>
        public int M<U>(Func<U, T, T> f, int x) => x;
    }

    /// <summary>
    /// Class B
    /// </summary>
    /// <typeparam name="T">T</typeparam>
    /// <remarks>Should show both inherited overloads for M&lt;U&gt;</remarks>
    public class B<T> : A<T>
    {
    }

    /// <summary>
    /// RandomGenericClass
    /// </summary>
    /// <typeparam name="TName">TName</typeparam>
    /// <typeparam name="TIndex">TIndex</typeparam>
    /// <typeparam name="TType">TType</typeparam>
    public class RandomGenericClass<TName, TIndex, TType>
    {
        internal RandomGenericClass()
        {
            Console.WriteLine(" I am constructing. ");
        }

        /// <summary>
        /// Documentation for Overload int.
        /// </summary>
        /// <param name="overload">The parameter.</param>
        /// <returns>The return.</returns>
        public int[] OverloadedMethod(int[] overload)
        {
            return overload;
        }

        /// <summary>
        /// Documentation for Overload single.
        /// </summary>
        /// <param name="overload">The parameter.</param>
        /// <returns>The return.</returns>
        public Single[] OverloadedMethod(Single[] overload)
        {
            return overload;
        }

        /// <summary>
        /// Documentation for Overload double.
        /// </summary>
        /// <param name="overload">The parameter.</param>
        /// <returns>The return</returns>
        public double[] OverloadedMethod(double[] overload)
        {
            return overload;
        }
    }

    /// <summary>
    /// InheritedRandomGeneric
    /// </summary>
    /// <remarks>Should show all of the inherited OverloadedMethod members</remarks>
    public class InheritedRandomGeneric : RandomGenericClass<String, int, float>
    {
    }

    /// <summary>
    /// MyDataSeries TX, TY
    /// </summary>
    /// <typeparam name="TX">TX</typeparam>
    /// <typeparam name="TY">TY</typeparam>
    public abstract class MyDataSeries<TX, TY>
        where TX : IComparable
        where TY : IComparable
    {
        /// <summary>
        /// Append
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="yValues">yValues</param>
        public virtual void Append(IEnumerable<TX> x, params IEnumerable<TY>[] yValues) { }

        /// <summary>
        /// Append
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="yValues">yValues</param>
        public virtual void Append(TX x, params TY[] yValues) { }
    }

    /// <summary>
    /// MyDataSeries TX, TY, TZ
    /// </summary>
    /// <typeparam name="TX">TX</typeparam>
    /// <typeparam name="TY">TY</typeparam>
    /// <typeparam name="TZ">TZ</typeparam>
    public class MyDataSeries<TX, TY, TZ> : MyDataSeries<TX, TY>
        where TX : IComparable
        where TY : IComparable
        where TZ : IComparable
    {
        /// <summary>
        /// Append
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="yValues">yValues</param>
        public override void Append(IEnumerable<TX> x, params IEnumerable<TY>[] yValues) { }

        /// <summary>
        /// Append
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="yValues">yValues</param>
        public override void Append(TX x, params TY[] yValues) { }
    }

    /// <summary>
    /// MyTimeDataSeries
    /// </summary>
    /// <remarks>Should show the inherited overloaded Append methods</remarks>
    public class MyTimeDataSeries : MyDataSeries<DateTime, double, double>
    {
    }

    /// <summary>
    /// Enthält eine Enumeration mit Status-Werten für die Rückgabe der GetXXX-Funktionen von <see cref="HGSettingsAccessBase{T}"/>.
    /// </summary>
    public enum HGSettingsStatus
    {
        /// <summary>
        /// Registry enthält die Zeichenfolge nicht
        /// </summary>
        NichtVorhanden = -1,
    }

    /// <summary>
    /// Basisklasse für beliebige Settings (Parameter <typeparamref name="T"/>), die in String-Form gespeichert werden und hier
    /// als Int, Double, Boolean, Date, Enum, Int-Liste oder ähnliches herausgegeben haben.
    /// 
    /// Subklassen müssen wenige Methoden für den konkreten Zugriff auf <typeparamref name="T"/> überschreiben, 
    /// der Rest (Wert-String-Konvertierungen und umgekehrt) geschieht hier ;-)
    /// 
    /// Designkonzept für Subklassen: sie implementieren die abstrakten Methoden, halten sich selbst Singleton,
    /// und geben die Getter/Setters als public static Methoden raus! 
    /// 
    /// KEINE Instanzen!
    /// </summary>
    /// <typeparam name="T">Beliebige Quelle für Einstellungen. Subklassen kümmern sich um den konkreten Wertezugriff.</typeparam>
    /// <remarks>
    /// 4.13.0     Klasse erfunden (WKnauf 24.11.2009)
    /// 4.14.9     "GetInt32Value" macht keine interne Exception bei Leerstring (WKnauf 04.02.2010)
    /// 8.10.0.4   Farbwerte werden jetzt als ARGB-Werte gespeichert statt nur als RGB (WKnauf 08.12.2014)
    /// 9.2.0.0    "GetStringListValue" etc. (WKnauf 02.12.2016)
    /// 9.5.0.0    "GetSingleValue" etc. (WKnauf 19.04.2017)
    /// 9.6.0.0    "GetGuidValue" etc. (WKnauf 26.09.2017)
    /// 10.0.0.0   Mantis 19493: "GetFontValue/SetFontValue": englische Culture verwenden (WKnauf 01.09.2021)
    /// </remarks>
    public abstract class HGSettingsAccessBase<T>
    where T : class
    {
        #region Protected Funktionen (Parametercheck)
        /// <summary>
        /// Prüft, ob die übergebene Einstellungen-Quelle NULL ist. Wenn ja, wird eine Exception geworfen.
        /// </summary>
        /// <param name="_settingsSource">Dieser Wert  darf net NULL sein</param>
        protected static void CheckRegKeyNotNull(T _settingsSource)
        {
        }

        /// <summary>
        /// Prüft, ob der übergebene Value-Name NULL oder leer ist. Wenn ja, wird eine Exception geworfen.
        /// </summary>
        /// <param name="_strValueName">Der zu prüfenden Value-Name</param>
        protected static void CheckRegValueNameNotNull(string _strValueName)
        {
        }

        #endregion

        #region Abstraktussen (allesamt Public, damit generische Verwendung möglich)
        /// <summary>
        /// Echt abstrakt: Vorhandensein eines Werts überprüfen.
        /// </summary>
        /// <remarks>
        /// Könnte besser "ContainsValue" heißen, aber das würde sich mit static Methoden in HGRegistryFunktionen beißen.
        /// Deshalb rausgelassen!
        /// </remarks>
        /// <param name="_settingsSource">Für diese Quelle wird geprüft, ob sie nen Wert enthält. Darf nicht NULL sein!</param>
        /// <param name="_strValueName">Dieser Wert wird auf Vorhandensein geprüft.</param>
        /// <returns>TRUE, wenn Value vorhanden ist. FALSE sonst.</returns>
        public abstract bool IsValueInSettings(T _settingsSource, string _strValueName);

        /// <summary>
        /// Übergebenen Wert in die Settings packen, nachdem er durch die internen Methoden konvertiert wurde.
        /// </summary>
        /// <param name="_settings">Settings</param>
        /// <param name="_strValueName">Unter diesem Value wird der Wert gesetzt.</param>
        /// <param name="_strValue">Zu setzender String-Wert</param>
        public abstract void SetValue(T _settings, string _strValueName, string _strValue);

        /// <summary>
        /// String-Wert aus den Settings auslesen. Später wird er durch interne Methoden konvertiert.
        /// </summary>
        /// <param name="_settings">Settings</param>
        /// <param name="_strValueName">Zu setzender String-Wert</param>
        /// <returns>Gefundener Wert oder NULL.</returns>
        public abstract string GetValue(T _settings, string _strValueName);

        /// <summary>
        /// String-Wert aus den Settings LÖSCHEN, z.B. bei Erkennung von kaputten Werten.
        /// 
        /// ACHTUNG: wenn die Settings das nicht erlauben, dann sollte nix kaputtgehen,
        /// </summary>
        /// <param name="_settings">Settings</param>
        /// <param name="_strValueName">Zu löschender String-Wert</param>
        public abstract void DeleteValue(T _settings, string _strValueName);
        #endregion

        #region Setzen/Holen von Datentypen

        #region Enum
        /// <summary>
        /// Schreibt einen Enum-Wert in die Settings. Es wird einfach nur "ToString" aufgerufen.
        /// </summary>
        /// <param name="_settings">Datenquelle, in die der Wert geschrieben werden soll. Darf nicht NULL sein.</param>
        /// <param name="_strValueName">Der Name des zu setzenden Werts. Ebenfalls nicht Null oder Leer.</param>
        /// <param name="_enumValue">Der zu setzende Wert. Es wird seine ToString-Methode aufgerufen</param>
        public void SetEnumValue(T _settings, string _strValueName, Enum _enumValue)
        {
        }
        /// <summary>
        /// Holt einen Enum-Wert aus der Settings.
        /// </summary>
        /// <param name="_settings">Datenquelle. Darf nicht NULL sein.</param>
        /// <param name="_strValueName">Der Name des zu holenden Werts. Ebenfalls nicht Null oder Leer.</param>
        /// <param name="_enumDefaultValue">Der Default-Wert, wenn kein Wert gefunden wurde.</param>
        /// <param name="_enumValue">In diesen Parameter wird der zu holende Wert herein geschrieben.
        /// Deshalb per Referenz übergeben. 
        /// ACHTUNG: Im Verwender muss eine Variable vom Typ "System.Enum" übergeben werden, die Referenz-Übergabe
        /// klappt sonst nicht (Compilefehler!)</param>
        /// <typeparam name="TEnum">Enum-Typ (leider nicht per Constraint als solcher deklarierbar)</typeparam>
        /// <returns>Werte der Enumeration EnumValueStatus (in den Fehlerfällen wird kein Fehler erzeugt, sondern der
        /// Default-Wert zurückgegeben!)
        /// Im Fehlerfall wird der ungültige Key gelöscht.</returns>
        public HGSettingsStatus GetEnumValue<TEnum>(T _settings, string _strValueName, TEnum _enumDefaultValue, out TEnum _enumValue)
          where TEnum : Enum
        {
            return GetEnumValue<TEnum>(_settings, _strValueName, _enumDefaultValue, out _enumValue, true);
        }

        /// <summary>
        /// Holt einen Enum-Wert aus der Settings.
        /// </summary>
        /// <param name="_settings">Datenquelle. Darf nicht NULL sein.</param>
        /// <param name="_strValueName">Der Name des zu holenden Werts. Ebenfalls nicht Null oder Leer.</param>
        /// <param name="_enumDefaultValue">Der Default-Wert, wenn kein Wert gefunden wurde.</param>
        /// <param name="_enumValue">In diesen Parameter wird der zu holende Wert herein geschrieben.
        /// Deshalb per Referenz übergeben. 
        /// ACHTUNG: Im Verwender muss eine Variable vom Typ "System.Enum" übergeben werden, die Referenz-Übergabe
        /// klappt sonst nicht (Compilefehler!)</param>
        /// <param name="_bolDeleteInvalid">TRUE: falls ungültiger Enum-Wert dann löschen. FALSE: nicht löschen (z.B. weil aus "app.config" geladen wird
        /// und diese schreibgeschützt ist)</param>
        /// <typeparam name="TEnum">Enum-Typ (leider nicht per Constraint als solcher deklarierbar)</typeparam>
        /// <returns>Werte der Enumeration EnumValueStatus (in den Fehlerfällen wird kein Fehler erzeugt, sondern der
        /// Default-Wert zurückgegeben!)
        /// Im Fehlerfall wird der ungültige Key gelöscht.</returns>
        public HGSettingsStatus GetEnumValue<TEnum>(T _settings, string _strValueName, TEnum _enumDefaultValue, out TEnum _enumValue, bool _bolDeleteInvalid)
          where TEnum : Enum
        {
            _enumValue = _enumDefaultValue;
            return HGSettingsStatus.NichtVorhanden;
        }
        #endregion

        #endregion
    }

    /// <summary>
    /// Klasse mit Funktionen für den Zugriff auf Anwendungs-Settings ("app.exe.config" bzw. "web.config", erreichbar über System.Configuration.ConfigurationManager.AppSettings).
    /// 
    /// 
    /// Sie ist abgeleitet von der allgemeinen Klasse <see cref="HGSettingsAccessBase{T}"/>, die die Konvertierung von Werten in/zu Strings
    /// übernimmt. Hier steckt nur der Code für das Auslesen/Setzen aus/in Einstellung, und außerdem werden hier ein Haufen 
    /// Static Methoden definiert, die alle auf die Hilfsmethoden der Basisklasse leiten!
    /// </summary>
    /// <remarks>
    /// 4.13.0     Klasse erfunden (WKnauf 24.11.2009)
    /// </remarks>
    public class HGAppSettings : HGSettingsAccessBase<NameValueCollection>
    {
        #region Unique Instance
        /// <summary>
        /// Abrufen der Unique Instance. Erzeugt sie nur beim ersten Abrufen.
        /// 
        /// </summary>
        public static HGAppSettings UniqueInstance => null;

        #endregion

        #region Konstruktor
        /// <summary>
        /// Privater Konstruktor, da keine Instanzen außer der Unique Instance erlaubt!
        /// </summary>
        private HGAppSettings()
        {
        }
        #endregion

        #region Abstraktes
        /// <summary>
        /// Methode prüft, ob im übergebenen Registry-Key der angegebene Value enthalten ist.
        /// </summary>
        /// <param name="_settings">Die <see cref="NameValueCollection"/>, in der der Value gesucht werden soll.</param>
        /// <param name="str_ValueName">Der zu suchende Value. Darf nicht NULL oder leer sein</param>
        /// <returns>tRUE, wenn Value in Key enthalten ist. FALSE sonst. Exception bei falschen Parametern.</returns>
        public override bool IsValueInSettings(NameValueCollection _settings, string str_ValueName)
        {
            return false;
        }


        /// <summary>
        /// Übergebenen Wert in die Settings packen, nachdem er durch die internen Methoden konvertiert wurde.
        /// </summary>
        /// <param name="_settings">Settings</param>
        /// <param name="_strValueName">Unter diesem Value wird der Wert gesetzt.</param>
        /// <param name="_strValue">Zu setzender String-Wert</param>
        public override void SetValue(NameValueCollection _settings, string _strValueName, string _strValue)
        {
        }

        /// <summary>
        /// String-Wert aus den Settings auslesen. Später wird er durch interne Methoden konvertiert.
        /// </summary>
        /// <param name="_settings">Settings</param>
        /// <param name="_strValueName">Abzurufender String-Wert</param>
        /// <returns>Gefundener Wert oder NULL. Hier Besonderheit: wenn Wert mehrfach in Settings auftaucht, dann komma-separiert.</returns>
        public override string GetValue(NameValueCollection _settings, string _strValueName)
        {
            return null;
        }

        /// <summary>
        /// String-Wert aus den Settings LÖSCHEN, z.B. bei Erkennung von kaputten Werten.
        /// 
        /// ACHTUNG: wenn die Settings das nicht erlauben, dann sollte nix kaputtgehen,
        /// </summary>
        /// <param name="_settings">Settings</param>
        /// <param name="_strValueName">Zu löschender String-Wert</param>
        public override void DeleteValue(NameValueCollection _settings, string _strValueName)
        {
        }
        #endregion

        #region Setzen/Holen von Datentypen

        /// <summary>
        /// Schreibt einen Enum-Wert in die Registry. Es wird einfach nur "ToString" aufgerufen.
        /// </summary>
        /// <param name="_settings">Datenquelle, in die der Wert geschrieben werden soll. Darf nicht NULL sein.</param>
        /// <param name="_strValueName">Der Name des zu setzenden Werts. Ebenfalls nicht Null oder Leer.</param>
        /// <param name="_enumValue">Der zu setzende Wert. Es wird seine ToString-Methode aufgerufen</param>
        public static void SetEnum(NameValueCollection _settings, string _strValueName, Enum _enumValue)
        {
        }

        /// <summary>
        /// Holt einen Enum-Wert aus der Registry.
        /// </summary>
        /// <param name="_settings">Datenquelle. Darf nicht NULL sein.</param>
        /// <param name="_strValueName">Der Name des zu holenden Werts. Ebenfalls nicht Null oder Leer.</param>
        /// <param name="_enumDefaultValue">Der Default-Wert, wenn kein Wert gefunden wurde.</param>
        /// <param name="_enumValue">[out] In diesen Parameter wird der zu holende Wert herein geschrieben.</param>
        /// <typeparam name="TEnum">Typ der Enum. Leider nicht per Constraint auf "Enum" festlegbar, wird aber zur Laufzeit geprüft! </typeparam>
        /// <returns>Werte der Enumeration EnumValueStatus (in den Fehlerfällen wird kein Fehler erzeugt, sondern der
        /// Default-Wert zurückgegeben!)
        /// Im Fehlerfall wird der ungültige Key gelöscht.</returns>
        public static HGSettingsStatus GetEnum<TEnum>(NameValueCollection _settings, string _strValueName, TEnum _enumDefaultValue, out TEnum _enumValue)
          where TEnum : Enum
        {
            _enumValue = _enumDefaultValue;
            return HGSettingsStatus.NichtVorhanden;
        }
        #endregion
    }
}
