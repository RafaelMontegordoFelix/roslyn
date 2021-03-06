' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.Diagnostics

Namespace Microsoft.CodeAnalysis.Editor.UnitTests.Diagnostics.AddMissingReference
    Public Class AddMissingReferenceTests
        Inherits AbstractCrossLanguageUserDiagnosticTest

        Private ReadOnly PresentationCoreAssemblyIdentity As AssemblyIdentity
        Private ReadOnly PresentationFrameworkAssemblyIdentity As AssemblyIdentity
        Private ReadOnly WindowsBaseAssemblyIdentity As AssemblyIdentity
        Private ReadOnly SystemXamlAssemblyIdentity As AssemblyIdentity

        Private ReadOnly PresentationCoreAssemblyPath As String
        Private ReadOnly PresentationFrameworkAssemblyPath As String
        Private ReadOnly WindowsBaseAssemblyPath As String
        Private ReadOnly SystemXamlAssemblyPath As String

        Sub New()
            PresentationCoreAssemblyIdentity = GlobalAssemblyCache.ResolvePartialName("PresentationCore", PresentationCoreAssemblyPath)
            PresentationFrameworkAssemblyIdentity = GlobalAssemblyCache.ResolvePartialName("PresentationFramework", PresentationFrameworkAssemblyPath)
            WindowsBaseAssemblyIdentity = GlobalAssemblyCache.ResolvePartialName("WindowsBase", WindowsBaseAssemblyPath)
            SystemXamlAssemblyIdentity = GlobalAssemblyCache.ResolvePartialName("System.Xaml", SystemXamlAssemblyPath)
        End Sub

        Friend Overrides Function CreateDiagnosticProviderAndFixer(workspace As Workspace, language As String) As Tuple(Of DiagnosticAnalyzer, CodeFixProvider)
            Dim fixer As CodeFixProvider =
                CType(If(language = LanguageNames.CSharp,
                   DirectCast(New Microsoft.CodeAnalysis.CSharp.CodeFixes.AddMissingReference.AddMissingReferenceCodeFixProvider(), CodeFixProvider),
                   DirectCast(New Microsoft.CodeAnalysis.VisualBasic.CodeFixes.AddMissingReference.AddMissingReferenceCodeFixProvider(), CodeFixProvider)), CodeFixProvider)

            Return Tuple.Create(Of DiagnosticAnalyzer, CodeFixProvider)(Nothing, fixer)
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddMissingReference)>
        Public Sub AddProjectReferenceBetweenCSharpProjects()
            TestAddProjectReference(<Workspace>
                                        <Project Language="C#" AssemblyName="ProjectA" CommonReferences="true">
                                            <Document>public class A { }</Document>
                                        </Project>
                                        <Project Language="C#" AssemblyName="ProjectB" CommonReferences="true">
                                            <ProjectReference>ProjectA</ProjectReference>
                                            <Document>public class B : A { }</Document>
                                        </Project>
                                        <Project Language="C#" AssemblyName="ProjectC" CommonReferences="true">
                                            <ProjectReference>ProjectB</ProjectReference>
                                            <Document>public class C : B$$ { }</Document>
                                        </Project>
                                    </Workspace>,
                                    "ProjectC", "ProjectA")
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddMissingReference)>
        Public Sub AddProjectReferenceBetweenVisualBasicProjects()
            TestAddProjectReference(<Workspace>
                                        <Project Language="Visual Basic" AssemblyName="ProjectA" CommonReferences="true">
                                            <Document>Public Class A : End Class</Document>
                                        </Project>
                                        <Project Language="Visual Basic" AssemblyName="ProjectB" CommonReferences="true">
                                            <ProjectReference>ProjectA</ProjectReference>
                                            <Document>Public Class B : Inherits A : End Class</Document>
                                        </Project>
                                        <Project Language="Visual Basic" AssemblyName="ProjectC" CommonReferences="true">
                                            <ProjectReference>ProjectB</ProjectReference>
                                            <Document>Public Class C : Inherits $$B : End Class</Document>
                                        </Project>
                                    </Workspace>,
                                    "ProjectC", "ProjectA")
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddMissingReference)>
        Public Sub AddProjectReferenceBetweenMixedLanguages1()
            TestAddProjectReference(<Workspace>
                                        <Project Language="C#" AssemblyName="ProjectA" CommonReferences="true">
                                            <Document>public class A { }</Document>
                                        </Project>
                                        <Project Language="Visual Basic" AssemblyName="ProjectB" CommonReferences="true">
                                            <ProjectReference>ProjectA</ProjectReference>
                                            <Document>Public Class B : Inherits A : End Class</Document>
                                        </Project>
                                        <Project Language="Visual Basic" AssemblyName="ProjectC" CommonReferences="true">
                                            <ProjectReference>ProjectB</ProjectReference>
                                            <Document>Public Class C : Inherits $$B : End Class</Document>
                                        </Project>
                                    </Workspace>,
                                    "ProjectC", "ProjectA")
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddMissingReference)>
        Public Sub AddProjectReferenceBetweenMixedLanguages2()
            TestAddProjectReference(<Workspace>
                                        <Project Language="Visual Basic" AssemblyName="ProjectA" CommonReferences="true">
                                            <Document>Public Class A : End Class</Document>
                                        </Project>
                                        <Project Language="C#" AssemblyName="ProjectB" CommonReferences="true">
                                            <ProjectReference>ProjectA</ProjectReference>
                                            <Document>public class B : A { }</Document>
                                        </Project>
                                        <Project Language="C#" AssemblyName="ProjectC" CommonReferences="true">
                                            <ProjectReference>ProjectB</ProjectReference>
                                            <Document>public class C : B$$ { }</Document>
                                        </Project>
                                    </Workspace>,
                                    "ProjectC", "ProjectA")
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddMissingReference)>
        Public Sub AddMetadataReferenceToVisualBasicProjectErrorCode30005()
            TestAddUnresolvedMetadataReference(
                <Workspace>
                    <Project Language="Visual Basic" AssemblyName="VBProject" CommonReferences="true">
                        <ProjectReference>VBProject2</ProjectReference>
                        <Document>
                            Public Class Foo
                                Sub M()
                                    AddHandler ModuleWithEvent.E$$, Sub() Exit Sub
                                End Sub
                            End Class
                        </Document>
                    </Project>
                    <Project Language="Visual Basic" AssemblyName="VBProject2" CommonReferences="true">
                        <MetadataReference><%= WindowsBaseAssemblyPath %></MetadataReference>
                        <Document>
                            Public Module ModuleWithEvent
                                Public Event E As System.Windows.PropertyChangedCallback
                            End Module
                        </Document>
                    </Project>
                </Workspace>,
                "VBProject", WindowsBaseAssemblyIdentity)
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddMissingReference)>
        Public Sub AddMetadataReferenceToVisualBasicProjectErrorCode30007()
            TestAddUnresolvedMetadataReference(
                <Workspace>
                    <Project Language="Visual Basic" AssemblyName="VBProject" CommonReferences="true">
                        <MetadataReference><%= PresentationCoreAssemblyPath %></MetadataReference>
                        <Document>Public Class Foo : Inherits System.Windows.UIElement$$ : End Class</Document>
                    </Project>
                </Workspace>,
                "VBProject", WindowsBaseAssemblyIdentity)
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddMissingReference)>
        Public Sub AddMetadataReferenceToVisualBasicProjectErrorCode30652()
            TestAddUnresolvedMetadataReference(
                <Workspace>
                    <Project Language="Visual Basic" AssemblyName="VBProject" CommonReferences="true">
                        <ProjectReference>VBProject2</ProjectReference>
                        <Document>
                            Public Class Foo
                                Sub M()
                                    ModuleWithEvent.E$$(Nothing)
                                End Sub
                            End Class
                        </Document>
                    </Project>
                    <Project Language="Visual Basic" AssemblyName="VBProject2" CommonReferences="true">
                        <MetadataReference><%= WindowsBaseAssemblyPath %></MetadataReference>
                        <Document>
                            Public Module ModuleWithEvent
                                Public Sub E(x As System.Windows.DependencyProperty) : End Sub
                            End Module
                        </Document>
                    </Project>
                </Workspace>,
                "VBProject", WindowsBaseAssemblyIdentity)
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddMissingReference)>
        Public Sub AddMetadataReferenceToCSharpProject()
            TestAddUnresolvedMetadataReference(
                <Workspace>
                    <Project Language="C#" AssemblyName="CSharpProject" CommonReferences="true">
                        <MetadataReference><%= PresentationCoreAssemblyPath %></MetadataReference>
                        <Document>public class Foo : System.Windows.UIElement$$ { }</Document>
                    </Project>
                </Workspace>,
                "CSharpProject", WindowsBaseAssemblyIdentity)
        End Sub
    End Class
End Namespace
