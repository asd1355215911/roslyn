// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using Microsoft.CodeAnalysis.Editor.CSharp.SignatureHelp;
using Microsoft.CodeAnalysis.Editor.UnitTests.SignatureHelp;
using Roslyn.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.CSharp.UnitTests.SignatureHelp
{
    public class AttributeSignatureHelpProviderTests : AbstractCSharpSignatureHelpProviderTests
    {
        internal override ISignatureHelpProvider CreateSignatureHelpProvider()
        {
            return new AttributeSignatureHelpProvider();
        }

        #region "Regular tests"

        [Fact, Trait(Traits.Feature, Traits.Features.SignatureHelp)]
        public void TestInvocationWithoutParameters()
        {
            var markup = @"
class SomethingAttribute : System.Attribute
{
}

[[|Something($$|]]
class D
{
}";

            var expectedOrderedItems = new List<SignatureHelpTestItem>();
            expectedOrderedItems.Add(new SignatureHelpTestItem("SomethingAttribute()", string.Empty, null, currentParameterIndex: 0));

            Test(markup, expectedOrderedItems);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.SignatureHelp)]
        public void TestInvocationWithoutParametersMethodXmlComments()
        {
            var markup = @"
class SomethingAttribute : System.Attribute
{
    /// <summary>Summary For Attribute</summary>
    public SomethingAttribute() { }
}

[[|Something($$|]]
class D
{
}";

            var expectedOrderedItems = new List<SignatureHelpTestItem>();
            expectedOrderedItems.Add(new SignatureHelpTestItem("SomethingAttribute()", "Summary For Attribute", null, currentParameterIndex: 0));

            Test(markup, expectedOrderedItems);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.SignatureHelp)]
        public void TestInvocationWithParametersOn1()
        {
            var markup = @"
class SomethingAttribute : System.Attribute
{
    public SomethingAttribute(int someInteger, string someString) { }
}

[[|Something($$|]]
class D
{
}";

            var expectedOrderedItems = new List<SignatureHelpTestItem>();
            expectedOrderedItems.Add(new SignatureHelpTestItem("SomethingAttribute(int someInteger, string someString)", string.Empty, string.Empty, currentParameterIndex: 0));

            Test(markup, expectedOrderedItems);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.SignatureHelp)]
        public void TestInvocationWithParametersXmlCommentsOn1()
        {
            var markup = @"
class SomethingAttribute : System.Attribute
{
    /// <summary>
    /// Summary For Attribute
    /// </summary>
    /// <param name=""someInteger"">Param someInteger</param>
    /// <param name=""someString"">Param someString</param>
    public SomethingAttribute(int someInteger, string someString) { }
}

[[|Something($$
|]class D
{
}";

            var expectedOrderedItems = new List<SignatureHelpTestItem>();
            expectedOrderedItems.Add(new SignatureHelpTestItem("SomethingAttribute(int someInteger, string someString)", "Summary For Attribute", "Param someInteger", currentParameterIndex: 0));

            Test(markup, expectedOrderedItems);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.SignatureHelp)]
        public void TestInvocationWithParametersOn2()
        {
            var markup = @"
class SomethingAttribute : System.Attribute
{
    public SomethingAttribute(int someInteger, string someString) { }
}

[[|Something(22, $$|]]
class D
{
}";
            var expectedOrderedItems = new List<SignatureHelpTestItem>();
            expectedOrderedItems.Add(new SignatureHelpTestItem("SomethingAttribute(int someInteger, string someString)", string.Empty, string.Empty, currentParameterIndex: 1));

            Test(markup, expectedOrderedItems);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.SignatureHelp)]
        public void TestInvocationWithParametersXmlComentsOn2()
        {
            var markup = @"
class SomethingAttribute : System.Attribute
{
    /// <summary>
    /// Summary For Attribute
    /// </summary>
    /// <param name=""someInteger"">Param someInteger</param>
    /// <param name=""someString"">Param someString</param>
    public SomethingAttribute(int someInteger, string someString) { }
}

[[|Something(22, $$
|]class D
{
}";

            var expectedOrderedItems = new List<SignatureHelpTestItem>();
            expectedOrderedItems.Add(new SignatureHelpTestItem("SomethingAttribute(int someInteger, string someString)", "Summary For Attribute", "Param someString", currentParameterIndex: 1));

            Test(markup, expectedOrderedItems);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.SignatureHelp)]
        public void TestInvocationWithClosingParen()
        {
            var markup = @"
class SomethingAttribute : System.Attribute
{ }

[[|Something($$|])]
class D
{
}";

            var expectedOrderedItems = new List<SignatureHelpTestItem>();
            expectedOrderedItems.Add(new SignatureHelpTestItem("SomethingAttribute()", string.Empty, null, currentParameterIndex: 0));

            Test(markup, expectedOrderedItems);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.SignatureHelp)]
        public void TestInvocationSpan1()
        {
            Test(
@"using System;

class C
{
    [[|Obsolete($$|])]
    void Foo()
    {
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.SignatureHelp)]
        public void TestInvocationSpan2()
        {
            Test(
@"using System;

class C
{
    [[|Obsolete(    $$|])]
    void Foo()
    {
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.SignatureHelp)]
        public void TestInvocationSpan3()
        {
            Test(
@"using System;

class C
{
    [[|Obsolete(

$$|]]
    void Foo()
    {
    }
}");
        }

        #endregion

        #region "Current Parameter Name"

        [Fact, Trait(Traits.Feature, Traits.Features.SignatureHelp)]
        public void TestCurrentParameterName()
        {
            var markup = @"
using System;

class SomethingAttribute : Attribute
{
    public SomethingAttribute(int someParameter, bool somethingElse) { }
}

[[|Something(somethingElse: false, someParameter: $$22|])]
class C
{
}";

            VerifyCurrentParameterName(markup, "someParameter");
        }

        #endregion

        #region "Setting fields in attributes"

        [WorkItem(545425)]
        [WorkItem(544139)]
        [Fact, Trait(Traits.Feature, Traits.Features.SignatureHelp)]
        public void TestAttributeWithValidField()
        {
            var markup = @"
class SomethingAttribute : System.Attribute
{
    public int foo;
}

[[|Something($$|])]
class D
{
}";

            var expectedOrderedItems = new List<SignatureHelpTestItem>();
            expectedOrderedItems.Add(new SignatureHelpTestItem("SomethingAttribute(Properties: [foo = int])", string.Empty, string.Empty, currentParameterIndex: 0));

            Test(markup, expectedOrderedItems, usePreviousCharAsTrigger: false);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.SignatureHelp)]
        public void TestAttributeWithInvalidFieldReadonly()
        {
            var markup = @"
class SomethingAttribute : System.Attribute
{
    public readonly int foo;
}

[[|Something($$|])]
class D
{
}";

            var expectedOrderedItems = new List<SignatureHelpTestItem>();
            expectedOrderedItems.Add(new SignatureHelpTestItem("SomethingAttribute()", string.Empty, null, currentParameterIndex: 0));

            Test(markup, expectedOrderedItems);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.SignatureHelp)]
        public void TestAttributeWithInvalidFieldStatic()
        {
            var markup = @"
class SomethingAttribute : System.Attribute
{
    public static int foo;
}

[[|Something($$|])]
class D
{
}";

            var expectedOrderedItems = new List<SignatureHelpTestItem>();
            expectedOrderedItems.Add(new SignatureHelpTestItem("SomethingAttribute()", string.Empty, null, currentParameterIndex: 0));

            Test(markup, expectedOrderedItems);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.SignatureHelp)]
        public void TestAttributeWithInvalidFieldConst()
        {
            var markup = @"
class SomethingAttribute : System.Attribute
{
    public const int foo = 42;
}

[[|Something($$|])]
class D
{
}";

            var expectedOrderedItems = new List<SignatureHelpTestItem>();
            expectedOrderedItems.Add(new SignatureHelpTestItem("SomethingAttribute()", string.Empty, null, currentParameterIndex: 0));

            Test(markup, expectedOrderedItems);
        }

        #endregion

        #region "Setting properties in attributes"

        [WorkItem(545425)]
        [WorkItem(544139)]
        [Fact, Trait(Traits.Feature, Traits.Features.SignatureHelp)]
        public void TestAttributeWithValidProperty()
        {
            var markup = @"
class SomethingAttribute : System.Attribute
{
    public int foo { get; set; }
}

[[|Something($$|])]
class D
{
}";

            var expectedOrderedItems = new List<SignatureHelpTestItem>();
            expectedOrderedItems.Add(new SignatureHelpTestItem("SomethingAttribute(Properties: [foo = int])", string.Empty, string.Empty, currentParameterIndex: 0));

            // TODO: Bug 12319: Enable tests for script when this is fixed.
            Test(markup, expectedOrderedItems, usePreviousCharAsTrigger: false);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.SignatureHelp)]
        public void TestAttributeWithInvalidPropertyStatic()
        {
            var markup = @"
class SomethingAttribute : System.Attribute
{
    public static int foo { get; set; }
}

[[|Something($$|])]
class D
{
}";

            var expectedOrderedItems = new List<SignatureHelpTestItem>();
            expectedOrderedItems.Add(new SignatureHelpTestItem("SomethingAttribute()", string.Empty, null, currentParameterIndex: 0));

            Test(markup, expectedOrderedItems);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.SignatureHelp)]
        public void TestAttributeWithInvalidPropertyNoSetter()
        {
            var markup = @"
class SomethingAttribute : System.Attribute
{
    public int foo { get { return 0; } }
}

[[|Something($$|])]
class D
{
}";

            var expectedOrderedItems = new List<SignatureHelpTestItem>();
            expectedOrderedItems.Add(new SignatureHelpTestItem("SomethingAttribute()", string.Empty, null, currentParameterIndex: 0));

            Test(markup, expectedOrderedItems);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.SignatureHelp)]
        public void TestAttributeWithInvalidPropertyNoGetter()
        {
            var markup = @"
class SomethingAttribute : System.Attribute
{
    public int foo { set { } }
}

[[|Something($$|])]
class D
{
}";

            var expectedOrderedItems = new List<SignatureHelpTestItem>();
            expectedOrderedItems.Add(new SignatureHelpTestItem("SomethingAttribute()", string.Empty, null, currentParameterIndex: 0));

            Test(markup, expectedOrderedItems);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.SignatureHelp)]
        public void TestAttributeWithInvalidPropertyPrivateGetter()
        {
            var markup = @"
class SomethingAttribute : System.Attribute
{
    public int foo { private get; set; }
}

[[|Something($$|])]
class D
{
}";

            var expectedOrderedItems = new List<SignatureHelpTestItem>();
            expectedOrderedItems.Add(new SignatureHelpTestItem("SomethingAttribute()", string.Empty, null, currentParameterIndex: 0));

            Test(markup, expectedOrderedItems);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.SignatureHelp)]
        public void TestAttributeWithInvalidPropertyPrivateSetter()
        {
            var markup = @"
class SomethingAttribute : System.Attribute
{
    public int foo { get; private set; }
}

[[|Something($$|])]
class D
{
}";

            var expectedOrderedItems = new List<SignatureHelpTestItem>();
            expectedOrderedItems.Add(new SignatureHelpTestItem("SomethingAttribute()", string.Empty, null, currentParameterIndex: 0));

            Test(markup, expectedOrderedItems);
        }

        #endregion

        #region "Setting fields and arguments"

        [WorkItem(545425)]
        [WorkItem(544139)]
        [Fact, Trait(Traits.Feature, Traits.Features.SignatureHelp)]
        public void TestAttributeWithArgumentsAndNamedParameters1()
        {
            var markup = @"
class SomethingAttribute : System.Attribute
{
    /// <param name=""foo"">FooParameter</param>
    /// <param name=""bar"">BarParameter</param>
    public SomethingAttribute(int foo = 0, string bar = null) { }
    public int fieldfoo { get; set; }
    public string fieldbar { get; set; }
}

[[|Something($$|])]
class D
{
}";

            var expectedOrderedItems = new List<SignatureHelpTestItem>();
            expectedOrderedItems.Add(new SignatureHelpTestItem("SomethingAttribute([int foo = 0], [string bar = null], Properties: [fieldbar = string], [fieldfoo = int])", string.Empty, "FooParameter", currentParameterIndex: 0));

            // TODO: Bug 12319: Enable tests for script when this is fixed.
            Test(markup, expectedOrderedItems, usePreviousCharAsTrigger: false);
        }

        [WorkItem(545425)]
        [WorkItem(544139)]
        [Fact, Trait(Traits.Feature, Traits.Features.SignatureHelp)]
        public void TestAttributeWithArgumentsAndNamedParameters2()
        {
            var markup = @"
class SomethingAttribute : System.Attribute
{
    /// <param name=""foo"">FooParameter</param>
    /// <param name=""bar"">BarParameter</param>
    public SomethingAttribute(int foo = 0, string bar = null) { }
    public int fieldfoo { get; set; }
    public string fieldbar { get; set; }
}

[[|Something(22, $$|])]
class D
{
}";

            var expectedOrderedItems = new List<SignatureHelpTestItem>();
            expectedOrderedItems.Add(new SignatureHelpTestItem("SomethingAttribute([int foo = 0], [string bar = null], Properties: [fieldbar = string], [fieldfoo = int])", string.Empty, "BarParameter", currentParameterIndex: 1));

            // TODO: Bug 12319: Enable tests for script when this is fixed.
            Test(markup, expectedOrderedItems, usePreviousCharAsTrigger: false);
        }

        [WorkItem(545425)]
        [WorkItem(544139)]
        [Fact, Trait(Traits.Feature, Traits.Features.SignatureHelp)]
        public void TestAttributeWithArgumentsAndNamedParameters3()
        {
            var markup = @"
class SomethingAttribute : System.Attribute
{
    /// <param name=""foo"">FooParameter</param>
    /// <param name=""bar"">BarParameter</param>
    public SomethingAttribute(int foo = 0, string bar = null) { }
    public int fieldfoo { get; set; }
    public string fieldbar { get; set; }
}

[[|Something(22, null, $$|])]
class D
{
}";

            var expectedOrderedItems = new List<SignatureHelpTestItem>();
            expectedOrderedItems.Add(new SignatureHelpTestItem("SomethingAttribute([int foo = 0], [string bar = null], Properties: [fieldbar = string], [fieldfoo = int])", string.Empty, string.Empty, currentParameterIndex: 2));

            // TODO: Bug 12319: Enable tests for script when this is fixed.
            Test(markup, expectedOrderedItems, usePreviousCharAsTrigger: false);
        }

        [WorkItem(545425)]
        [WorkItem(544139)]
        [Fact, Trait(Traits.Feature, Traits.Features.SignatureHelp)]
        public void TestAttributeWithOptionalArgumentAndNamedParameterWithSameName1()
        {
            var markup = @"
class SomethingAttribute : System.Attribute
{
    /// <param name=""foo"">FooParameter</param>
    public SomethingAttribute(int foo = 0) { }
    public int foo { get; set; }
}

[[|Something($$|])]
class D
{
}";

            var expectedOrderedItems = new List<SignatureHelpTestItem>();
            expectedOrderedItems.Add(new SignatureHelpTestItem("SomethingAttribute([int foo = 0], Properties: [foo = int])", string.Empty, "FooParameter", currentParameterIndex: 0));

            // TODO: Bug 12319: Enable tests for script when this is fixed.
            Test(markup, expectedOrderedItems, usePreviousCharAsTrigger: false);
        }

        [WorkItem(545425)]
        [WorkItem(544139)]
        [Fact, Trait(Traits.Feature, Traits.Features.SignatureHelp)]
        public void TestAttributeWithOptionalArgumentAndNamedParameterWithSameName2()
        {
            var markup = @"
class SomethingAttribute : System.Attribute
{
    /// <param name=""foo"">FooParameter</param>
    public SomethingAttribute(int foo = 0) { }
    public int foo { get; set; }
}

[[|Something(22, $$|])]
class D
{
}";

            var expectedOrderedItems = new List<SignatureHelpTestItem>();
            expectedOrderedItems.Add(new SignatureHelpTestItem("SomethingAttribute([int foo = 0], Properties: [foo = int])", string.Empty, string.Empty, currentParameterIndex: 1));

            // TODO: Bug 12319: Enable tests for script when this is fixed.
            Test(markup, expectedOrderedItems, usePreviousCharAsTrigger: false);
        }

        #endregion

        #region "Trigger tests"

        [Fact, Trait(Traits.Feature, Traits.Features.SignatureHelp)]
        public void TestInvocationOnTriggerParens()
        {
            var markup = @"
using System;

class SomethingAttribute : Attribute
{
    public SomethingAttribute(int someParameter, bool somethingElse) { }
}

[[|Something($$|])]
class C
{
}";

            var expectedOrderedItems = new List<SignatureHelpTestItem>();
            expectedOrderedItems.Add(new SignatureHelpTestItem("SomethingAttribute(int someParameter, bool somethingElse)", string.Empty, string.Empty, currentParameterIndex: 0));

            Test(markup, expectedOrderedItems, usePreviousCharAsTrigger: true);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.SignatureHelp)]
        public void TestInvocationOnTriggerComma()
        {
            var markup = @"
using System;

class SomethingAttribute : Attribute
{
    public SomethingAttribute(int someParameter, bool somethingElse) { }
}

[[|Something(22,$$|])]
class C
{
}";

            var expectedOrderedItems = new List<SignatureHelpTestItem>();
            expectedOrderedItems.Add(new SignatureHelpTestItem("SomethingAttribute(int someParameter, bool somethingElse)", string.Empty, string.Empty, currentParameterIndex: 1));

            Test(markup, expectedOrderedItems, usePreviousCharAsTrigger: true);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.SignatureHelp)]
        public void TestNoInvocationOnSpace()
        {
            var markup = @"
using System;

class SomethingAttribute : Attribute
{
    public SomethingAttribute(int someParameter, bool somethingElse) { }
}

[[|Something(22, $$|])]
class C
{
}";

            var expectedOrderedItems = new List<SignatureHelpTestItem>();
            Test(markup, expectedOrderedItems, usePreviousCharAsTrigger: true);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.SignatureHelp)]
        public void TestTriggerCharacters()
        {
            char[] expectedCharacters = { ',', '(' };
            char[] unexpectedCharacters = { ' ', '[', '<' };

            VerifyTriggerCharacters(expectedCharacters, unexpectedCharacters);
        }

        #endregion

        #region "EditorBrowsable tests"
        [WorkItem(7336, "DevDiv_Projects/Roslyn")]
        [Fact, Trait(Traits.Feature, Traits.Features.SignatureHelp)]
        public void EditorBrowsable_Attribute_BrowsableAlways()
        {
            var markup = @"
[MyAttribute($$
class Program
{
}";

            var referencedCode = @"

public class MyAttribute
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Always)]
    public MyAttribute(int x)
    {
    }
}";

            var expectedOrderedItems = new List<SignatureHelpTestItem>();
            expectedOrderedItems.Add(new SignatureHelpTestItem("MyAttribute(int x)", string.Empty, string.Empty, currentParameterIndex: 0));

            TestSignatureHelpInEditorBrowsableContexts(markup: markup,
                                                referencedCode: referencedCode,
                                                expectedOrderedItemsMetadataReference: expectedOrderedItems,
                                                expectedOrderedItemsSameSolution: expectedOrderedItems,
                                                sourceLanguage: LanguageNames.CSharp,
                                                referencedLanguage: LanguageNames.CSharp);
        }

        [WorkItem(7336, "DevDiv_Projects/Roslyn")]
        [Fact, Trait(Traits.Feature, Traits.Features.SignatureHelp)]
        public void EditorBrowsable_Attribute_BrowsableNever()
        {
            var markup = @"
[MyAttribute($$
class Program
{
}";

            var referencedCode = @"

public class MyAttribute
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public MyAttribute(int x)
    {
    }
}";

            var expectedOrderedItems = new List<SignatureHelpTestItem>();
            expectedOrderedItems.Add(new SignatureHelpTestItem("MyAttribute(int x)", string.Empty, string.Empty, currentParameterIndex: 0));

            TestSignatureHelpInEditorBrowsableContexts(markup: markup,
                                                referencedCode: referencedCode,
                                                expectedOrderedItemsMetadataReference: new List<SignatureHelpTestItem>(),
                                                expectedOrderedItemsSameSolution: expectedOrderedItems,
                                                sourceLanguage: LanguageNames.CSharp,
                                                referencedLanguage: LanguageNames.CSharp);
        }

        [WorkItem(7336, "DevDiv_Projects/Roslyn")]
        [Fact, Trait(Traits.Feature, Traits.Features.SignatureHelp)]
        public void EditorBrowsable_Attribute_BrowsableAdvanced()
        {
            var markup = @"
[MyAttribute($$
class Program
{
}";

            var referencedCode = @"

public class MyAttribute
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
    public MyAttribute(int x)
    {
    }
}";

            var expectedOrderedItems = new List<SignatureHelpTestItem>();
            expectedOrderedItems.Add(new SignatureHelpTestItem("MyAttribute(int x)", string.Empty, string.Empty, currentParameterIndex: 0));

            TestSignatureHelpInEditorBrowsableContexts(markup: markup,
                                                referencedCode: referencedCode,
                                                expectedOrderedItemsMetadataReference: new List<SignatureHelpTestItem>(),
                                                expectedOrderedItemsSameSolution: expectedOrderedItems,
                                                sourceLanguage: LanguageNames.CSharp,
                                                referencedLanguage: LanguageNames.CSharp,
                                                hideAdvancedMembers: true);

            TestSignatureHelpInEditorBrowsableContexts(markup: markup,
                                                referencedCode: referencedCode,
                                                expectedOrderedItemsMetadataReference: expectedOrderedItems,
                                                expectedOrderedItemsSameSolution: expectedOrderedItems,
                                                sourceLanguage: LanguageNames.CSharp,
                                                referencedLanguage: LanguageNames.CSharp,
                                                hideAdvancedMembers: false);
        }

        [WorkItem(7336, "DevDiv_Projects/Roslyn")]
        [Fact, Trait(Traits.Feature, Traits.Features.SignatureHelp)]
        public void EditorBrowsable_Attribute_BrowsableMixed()
        {
            var markup = @"
[MyAttribute($$
class Program
{
}";

            var referencedCode = @"

public class MyAttribute
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Always)]
    public MyAttribute(int x)
    {
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public MyAttribute(int x, int y)
    {
    }
}";

            var expectedOrderedItemsMetadataReference = new List<SignatureHelpTestItem>();
            expectedOrderedItemsMetadataReference.Add(new SignatureHelpTestItem("MyAttribute(int x)", string.Empty, string.Empty, currentParameterIndex: 0));

            var expectedOrderedItemsSameSolution = new List<SignatureHelpTestItem>();
            expectedOrderedItemsSameSolution.Add(new SignatureHelpTestItem("MyAttribute(int x)", string.Empty, string.Empty, currentParameterIndex: 0));
            expectedOrderedItemsSameSolution.Add(new SignatureHelpTestItem("MyAttribute(int x, int y)", string.Empty, string.Empty, currentParameterIndex: 0));

            TestSignatureHelpInEditorBrowsableContexts(markup: markup,
                                                referencedCode: referencedCode,
                                                expectedOrderedItemsMetadataReference: expectedOrderedItemsMetadataReference,
                                                expectedOrderedItemsSameSolution: expectedOrderedItemsSameSolution,
                                                sourceLanguage: LanguageNames.CSharp,
                                                referencedLanguage: LanguageNames.CSharp);
        }

        #endregion

        [Fact, Trait(Traits.Feature, Traits.Features.SignatureHelp)]
        public void FieldUnavailableInOneLinkedFile()
        {
            var markup = @"<Workspace>
    <Project Language=""C#"" CommonReferences=""true"" AssemblyName=""Proj1"" PreprocessorSymbols=""FOO"">
        <Document FilePath=""SourceDocument""><![CDATA[
class C
{
#if FOO
    class Secret : System.Attribute
    {
    }
#endif
    [Secret($$
    void Foo()
    {
    }
}
]]>
        </Document>
    </Project>
    <Project Language=""C#"" CommonReferences=""true"" AssemblyName=""Proj2"">
        <Document IsLinkFile=""true"" LinkAssemblyName=""Proj1"" LinkFilePath=""SourceDocument""/>
    </Project>
</Workspace>";
            var expectedDescription = new SignatureHelpTestItem("Secret()\r\n\r\n    Proj1 - Available\r\n    Proj2 - Not Available\r\n\r\nYou can use the navigation bar to switch context.", currentParameterIndex: 0);
            VerifyItemWithReferenceWorker(markup, new[] { expectedDescription }, false);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.SignatureHelp)]
        public void ExcludeFilesWithInactiveRegions()
        {
            var markup = @"<Workspace>
    <Project Language=""C#"" CommonReferences=""true"" AssemblyName=""Proj1"" PreprocessorSymbols=""FOO,BAR"">
        <Document FilePath=""SourceDocument""><![CDATA[
class C
{
#if FOO
    class Secret : System.Attribute
    {
    }
#endif

#if BAR
    [Secret($$
    void Foo()
    {
    }
#endif
}
]]>
        </Document>
    </Project>
    <Project Language=""C#"" CommonReferences=""true"" AssemblyName=""Proj2"">
        <Document IsLinkFile=""true"" LinkAssemblyName=""Proj1"" LinkFilePath=""SourceDocument"" />
    </Project>
    <Project Language=""C#"" CommonReferences=""true"" AssemblyName=""Proj3"" PreprocessorSymbols=""BAR"">
        <Document IsLinkFile=""true"" LinkAssemblyName=""Proj1"" LinkFilePath=""SourceDocument""/>
    </Project>
</Workspace>";

            var expectedDescription = new SignatureHelpTestItem("Secret()\r\n\r\n    Proj1 - Available\r\n    Proj3 - Not Available\r\n\r\nYou can use the navigation bar to switch context.", currentParameterIndex: 0);
            VerifyItemWithReferenceWorker(markup, new[] { expectedDescription }, false);
        }

        [WorkItem(1067933)]
        [Fact, Trait(Traits.Feature, Traits.Features.SignatureHelp)]
        public void InvokedWithNoToken()
        {
            var markup = @"
// [foo($$";

            Test(markup);
        }

        [WorkItem(1081535)]
        [Fact, Trait(Traits.Feature, Traits.Features.SignatureHelp)]
        public void TestInvocationWithBadParameterList()
        {
            var markup = @"
class SomethingAttribute : System.Attribute
{
}

[Something{$$]
class D
{
}";

            var expectedOrderedItems = new List<SignatureHelpTestItem>();
            Test(markup, expectedOrderedItems);
        }
    }
}
