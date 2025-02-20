using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using SMAPI.ModBuildConfig.Analyzer.Tests.Framework;
using StardewModdingAPI.ModBuildConfig.Analyzer;

namespace SMAPI.ModBuildConfig.Analyzer.Tests;

/// <summary>Unit tests for <see cref="NetFieldAnalyzer"/>.</summary>
[TestFixture]
public class NetFieldAnalyzerTests : DiagnosticVerifier
{
    /*********
    ** Fields
    *********/
    /// <summary>Sample C# mod code, with a {{test-code}} placeholder for the code in the Entry method to test.</summary>
    const string SampleProgram = @"
        using System;
        using StardewValley;
        using Netcode;
        using SObject = StardewValley.Object;

        namespace SampleMod
        {
            class ModEntry
            {
                public void Entry()
                {
                    {{test-code}}
                }
            }
        }
    ";

    /// <summary>The line number where the unit tested code is injected into <see cref="SampleProgram"/>.</summary>
    private const int SampleCodeLine = 13;

    /// <summary>The column number where the unit tested code is injected into <see cref="SampleProgram"/>.</summary>
    private const int SampleCodeColumn = 25;


    /*********
    ** Unit tests
    *********/
    /// <summary>Test that no diagnostics are raised for an empty code block.</summary>
    [TestCase]
    public void EmptyCode_HasNoDiagnostics()
    {
        // arrange
        string test = @"";

        // assert
        this.VerifyCSharpDiagnostic(test);
    }

    /// <summary>Test that the net field analyzer doesn't raise any warnings for safe member access.</summary>
    /// <param name="codeText">The code line to test.</param>
    [TestCase("Item item = new Item(); System.Collections.IEnumerable list = farmer.eventsSeen;")]
    [TestCase("Item item = new Item(); System.Collections.Generic.IEnumerable<int> list = farmer.netList;")]
    [TestCase("Item item = new Item(); System.Collections.Generic.IList<int> list = farmer.netList;")]
    [TestCase("Item item = new Item(); System.Collections.Generic.ICollection<int> list = farmer.netCollection;")]
    [TestCase("Item item = new Item(); System.Collections.Generic.IList<int> list = farmer.netObjectList;")] // subclass of NetList
    public void AvoidImplicitNetFieldComparisons_AllowsSafeAccess(string codeText)
    {
        // arrange
        string code = NetFieldAnalyzerTests.SampleProgram.Replace("{{test-code}}", codeText);

        // assert
        this.VerifyCSharpDiagnostic(code);
    }

    /// <summary>Test that the expected diagnostic message is raised for avoidable net field references.</summary>
    /// <param name="codeText">The code line to test.</param>
    /// <param name="column">The column within the code line where the diagnostic message should be reported.</param>
    /// <param name="expression">The expression which should be reported.</param>
    /// <param name="netType">The net type name which should be reported.</param>
    /// <param name="suggestedProperty">The suggested property name which should be reported.</param>
    [TestCase("Item item = null; int category = item.category;", 33, "item.category", "NetInt", "Category")]
    [TestCase("Item item = null; int category = (item).category;", 33, "(item).category", "NetInt", "Category")]
    [TestCase("Item item = null; int category = ((Item)item).category;", 33, "((Item)item).category", "NetInt", "Category")]
    [TestCase("SObject obj = null; int category = obj.category;", 35, "obj.category", "NetInt", "Category")]
    public void AvoidNetFields_RaisesDiagnostic(string codeText, int column, string expression, string netType, string suggestedProperty)
    {
        // arrange
        string code = NetFieldAnalyzerTests.SampleProgram.Replace("{{test-code}}", codeText);
        DiagnosticResult expected = new()
        {
            Id = "AvoidNetField",
            Message = $"'{expression}' is a {netType} field; consider using the {suggestedProperty} property instead. See https://smapi.io/package/avoid-net-field for details.",
            Severity = DiagnosticSeverity.Warning,
            Locations = new[] { new DiagnosticResultLocation("Test0.cs", NetFieldAnalyzerTests.SampleCodeLine, NetFieldAnalyzerTests.SampleCodeColumn + column) }
        };

        // assert
        this.VerifyCSharpDiagnostic(code, expected);
    }


    /*********
    ** Helpers
    *********/
    /// <summary>Get the analyzer being tested.</summary>
    protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
    {
        return new NetFieldAnalyzer();
    }
}
