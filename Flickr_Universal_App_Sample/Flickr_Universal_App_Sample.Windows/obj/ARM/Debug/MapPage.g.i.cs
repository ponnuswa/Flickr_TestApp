﻿

#pragma checksum "C:\Users\ponnuswa\documents\visual studio 2013\Projects\Flickr_Universal_App_Sample\Flickr_Universal_App_Sample\Flickr_Universal_App_Sample.Shared\MapPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "72EC26BAF0825E89F7C85DFA5BB550FD"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Flickr_Universal_App_Sample
{
    partial class MapPage : global::Windows.UI.Xaml.Controls.Page
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 4.0.0.0")]
        private global::Windows.UI.Xaml.Controls.AppBarButton BackButton; 
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 4.0.0.0")]
        private global::Flickr_Universal_App_Sample.MapView MyMap; 
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 4.0.0.0")]
        private bool _contentLoaded;

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 4.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent()
        {
            if (_contentLoaded)
                return;

            _contentLoaded = true;
            global::Windows.UI.Xaml.Application.LoadComponent(this, new global::System.Uri("ms-appx:///MapPage.xaml"), global::Windows.UI.Xaml.Controls.Primitives.ComponentResourceLocation.Application);
 
            BackButton = (global::Windows.UI.Xaml.Controls.AppBarButton)this.FindName("BackButton");
            MyMap = (global::Flickr_Universal_App_Sample.MapView)this.FindName("MyMap");
        }
    }
}


