using System;

namespace TestDoc
{
    // Record types are generated like normal classes and aren't marked with an attribute or anything that I can
    // find that can be easily used to distinguish them.  SHFB could probably look for the common compiler
    // generated members (Deconstruct, ToString, and equality members) and if present assume it's a record but
    // that's a lot of work and compiler generated members are omitted by default so that would have to be
    // implemented in MRefBuilder prior to them being filtered out.

    /// <summary>
    /// A person record
    /// </summary>
    /// <param name="FirstName">The first name</param>
    /// <param name="LastName">The last name</param>
    public record Person(string FirstName, string LastName);

    /// <summary>
    /// A point record
    /// </summary>
    /// <param name="X">X coordinate</param>
    /// <param name="Y">Y coordinate</param>
    /// <param name="Z">Z coordinate</param>
    public readonly record struct Point(double X, double Y, double Z);

    /// <summary>
    /// A mutable record
    /// </summary>
    /// <param name="TakenAt">The Taken At value</param>
    /// <param name="Measurement">The measurement</param>
    public record struct DataMeasurement(DateTime TakenAt, double Measurement);
}
