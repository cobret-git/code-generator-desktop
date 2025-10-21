using CommunityToolkit.Mvvm.ComponentModel;
using QuickCode.Components.Data;
using System;
using System.ComponentModel;

namespace QuickCode.ViewModel
{
    public partial class CodeGeneratorViewModel : ObservableObject
    {
        #region Fields
        [ObservableProperty] private CodeGenerationType codeGenType = CodeGenerationType.Link;
        #endregion

        #region Properties
        public CodeGenerationType[] CodeGenerationTypeOptions { get; } = Enum.GetValues<CodeGenerationType>();
        #endregion
    }
}
