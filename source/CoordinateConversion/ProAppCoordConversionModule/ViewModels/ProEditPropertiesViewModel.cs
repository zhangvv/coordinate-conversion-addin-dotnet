﻿
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using CoordinateConversionLibrary;
using CoordinateConversionLibrary.Helpers;
using CoordinateConversionLibrary.Models;
using ProAppCoordConversionModule.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
namespace ProAppCoordConversionModule.ViewModels
{
    class ProEditPropertiesViewModel : ProTabBaseViewModel
    {
        public ProEditPropertiesViewModel()
        {
            IsInitialCall = true;
            ArrowRotation = 0M;
            FormatList = new ObservableCollection<string>() { "One", "Two", "Three", "Four", "Five", "Six", "Custom" };
            Sample = "Sample";
            var removedType = new string[] { "Custom", "None" };
            IsEnableExpander = false;
            var coordinateCollections = Enum.GetValues(typeof(CoordinateTypes)).Cast<CoordinateTypes>().Where(x => !removedType.Contains(x.ToString()));
            CoordinateTypeCollections = new ObservableCollection<CoordinateTypes>(coordinateCollections);
            DefaultFormats = CoordinateConversionLibraryConfig.AddInConfig.DefaultFormatList;

            SelectedCoordinateType = CoordinateConversionLibraryConfig.AddInConfig.DisplayCoordinateType;
            DisplayAmbiguousCoordsDlg = CoordinateConversionLibraryConfig.AddInConfig.DisplayAmbiguousCoordsDlg;
            OKButtonPressedCommand = new RelayCommand(OnOkButtonPressedCommand);
            CancelButtonPressedCommand = new RelayCommand(OnCancelButtonPressedCommand);
            SearchResultCommand = new RelayCommand(OnSearchResultCommand);
            SelectedButtonCommand = new RelayCommand(OnSelectedButtonCommand);
            CancelButtonCommand = new RelayCommand(OnCancelButtonCommand);
            ApplyButtonCommand = new RelayCommand(OnApplyButtonCommand);
            FormatSelection = CoordinateConversionLibraryConfig.AddInConfig.FormatSelection;
            if (FormatSelection == CoordinateConversionLibrary.Properties.Resources.CustomString)
            {
                _categorySelection = FormatList.Where(x => x == CoordinateConversionLibrary.Properties.Resources.CustomString).FirstOrDefault();
                FormatExpanded = true;
                IsEnableExpander = true;
                Format = CoordinateBase.InputCustomFormat;
            }
            else
            {
                _categorySelection = FormatList.FirstOrDefault();
                FormatExpanded = false;
                IsEnableExpander = false;
            }
            if (AllSymbolCollection == null)
                AllSymbolCollection = new Dictionary<string, ObservableCollection<Symbol>>();

            ShowCheckBoxes();
            IsInitialCall = false;
        }

        #region Properties
        private Visibility _showLoadingProcess;

        public Visibility ShowLoadingProcess
        {
            get { return _showLoadingProcess; }
            set
            {
                _showLoadingProcess = value;
                ShowControls = (value == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
                RaisePropertyChanged(() => ShowLoadingProcess);
            }
        }

        private Visibility _showControls;

        public Visibility ShowControls
        {
            get { return _showControls; }
            set
            {
                _showControls = value;
                RaisePropertyChanged(() => ShowControls);
            }
        }

        private decimal _arrowRotation;

        public decimal ArrowRotation
        {
            get { return _arrowRotation; }
            set
            {
                _arrowRotation = value;
                RaisePropertyChanged(() => ArrowRotation);
            }
        }


        private int _selectedTabIndex;

        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set
            {
                _selectedTabIndex = value;
                OnTabSelectionChanged();
                RaisePropertyChanged(() => SelectedTabIndex);
            }
        }

        public bool IsInitialCall { get; set; }

        private bool _isPopUpOpen;
        public bool IsPopUpOpen
        {
            get { return _isPopUpOpen; }
            set
            {
                _isPopUpOpen = value;
                ArrowRotation = _isPopUpOpen ? 180 : 0;
                RaisePropertyChanged(() => IsPopUpOpen);
            }
        }

        private ObservableCollection<PropertyInfo> _colorPickerCollection { get; set; }
        public ObservableCollection<PropertyInfo> ColorPickerCollection
        {
            get
            {
                return _colorPickerCollection;
            }
            set
            {
                _colorPickerCollection = value;
                RaisePropertyChanged(() => ColorPickerCollection);
            }
        }

        private PropertyInfo _selectedColor { get; set; }
        public PropertyInfo SelectedColor
        {
            get
            {
                return _selectedColor;
            }
            set
            {
                _selectedColor = value;
                if (SelectedStyleItem != null)
                {
                    QueuedTask.Run(() =>
                    {
                        SelectedBrush = (System.Windows.Media.Brush)SelectedColor.GetValue(null, null);
                        var sym = ((ArcGIS.Core.CIM.CIMPointSymbol)(SelectedStyleItem.SymbolItem.Symbol));
                        var _color = ((System.Windows.Media.SolidColorBrush)(SelectedBrush)).Color;
                        sym.SetColor(CIMColor.CreateRGBColor(_color.R, _color.G, _color.B));
                        SelectedStyleItem.SymbolItem.Symbol = sym;
                        var image = SelectedStyleItem.SymbolItem.PreviewImage;
                        SelectedSymbolImage = SelectedStyleItem.SymbolItem.PreviewImage as BitmapImage;
                        SelectedSymbolText = SelectedStyleItem.SymbolText;
                    });
                }
                IsPopUpOpen = false;
                RaisePropertyChanged(() => SelectedColor);
            }
        }
        private System.Windows.Media.Brush _selectedBrush;
        public System.Windows.Media.Brush SelectedBrush
        {
            get { return _selectedBrush; }
            set
            {
                _selectedBrush = value;
                RaisePropertyChanged(() => SelectedBrush);
            }
        }

        private BitmapImage _selectedSymbolImage { get; set; }
        public BitmapImage SelectedSymbolImage
        {
            get
            {
                return _selectedSymbolImage;
            }
            set
            {
                _selectedSymbolImage = value;
                RaisePropertyChanged(() => SelectedSymbolImage);
            }
        }
        private string _selectedSymbolText { get; set; }
        public string SelectedSymbolText
        {
            get
            {
                return _selectedSymbolText;
            }
            set
            {
                _selectedSymbolText = value;
                RaisePropertyChanged(() => SelectedSymbolText);
            }
        }

        private Symbol _selectedStyleItem { get; set; }
        public Symbol SelectedStyleItem
        {
            get
            {
                return _selectedStyleItem;
            }
            set
            {
                _selectedStyleItem = value;
                if (value != null)
                {
                    QueuedTask.Run(() =>
                    {
                        var sym = ((ArcGIS.Core.CIM.CIMPointSymbol)(value.SymbolItem.Symbol));
                        var _color = ((System.Windows.Media.SolidColorBrush)(SelectedBrush)).Color;
                        sym.SetColor(CIMColor.CreateRGBColor(_color.R, _color.G, _color.B));
                        value.SymbolItem.Symbol = sym;
                        SelectedSymbolImage = value.SymbolItem.PreviewImage as BitmapImage;
                        SelectedSymbolText = value.SymbolText;
                    });
                }
            }
        }

        public ObservableCollection<Symbol> AllSymbolCollections;

        private ObservableCollection<Symbol> _symbolCollections;
        public ObservableCollection<Symbol> SymbolCollections
        {
            get { return _symbolCollections; }
            set
            {
                _symbolCollections = value;
                RaisePropertyChanged(() => SymbolCollections);
            }
        }

        public RelayCommand SelectedButtonCommand { get; set; }
        public RelayCommand SearchResultCommand { get; set; }
        public RelayCommand OKButtonPressedCommand { get; set; }
        public RelayCommand CancelButtonCommand { get; set; }
        public RelayCommand ApplyButtonCommand { get; set; }
        public RelayCommand CancelButtonPressedCommand { get; set; }
        private ObservableCollection<CoordinateTypes> _coordinateTypeCollections;
        public ObservableCollection<CoordinateTypes> CoordinateTypeCollections
        {
            get
            {
                return _coordinateTypeCollections;
            }
            set
            {
                _coordinateTypeCollections = value;
                RaisePropertyChanged(() => CoordinateTypeCollections);
            }
        }

        private CoordinateTypes _selectedCoordinateType { get; set; }
        public CoordinateTypes SelectedCoordinateType
        {
            get
            {
                return _selectedCoordinateType;
            }
            set
            {
                _selectedCoordinateType = value;
                CoordinateBase.InputCategorySelection = value;
                if (!IsInitialCall)
                    CoordinateConversionLibraryConfig.AddInConfig.IsCustomFormat = false;
                OnCategorySelectionChanged();
                RaisePropertyChanged(() => SelectedCoordinateType);
            }
        }

        private string _format = string.Empty;
        public string Format
        {
            get
            {
                return _format;
            }

            set
            {
                if (IsNotValidInput(value))
                {
                    IsValidFormat = false;
                    throw new ArgumentException(CoordinateConversionLibrary.Properties.Resources.SpecialCharactersValidationMsg);
                }
                else
                {
                    _format = value;
                    IsValidFormat = true;
                    RaisePropertyChanged(() => Format);
                }
            }
        }

        private string _formatSelection;
        public string FormatSelection
        {
            get
            {
                return _formatSelection;
            }
            set
            {
                if (_formatSelection != value)
                {
                    _formatSelection = value;
                    CoordinateBase.InputFormatSelection = value;
                    OnFormatSelectionChanged();
                    RaisePropertyChanged(() => FormatSelection);
                }
            }
        }

        private string _categorySelection = string.Empty;
        public string CategorySelection
        {
            get
            {
                return _categorySelection;
            }
            set
            {
                if (_categorySelection != value)
                {
                    _categorySelection = value;
                    OnCategorySelectionChanged();
                }
            }
        }

        public ObservableCollection<string> CategoryList { get; set; }
        public ObservableCollection<string> FormatList { get; set; }
        private static Dictionary<CoordinateType, string> ctdict = new Dictionary<CoordinateType, string>();
        public string Sample { get; set; }
        public bool IsValidFormat { get; set; }
        private bool _formatExpanded { get; set; }
        public bool FormatExpanded
        {
            get
            {
                return _formatExpanded;
            }
            set
            {
                _formatExpanded = value;
                RaisePropertyChanged(() => FormatExpanded);
            }
        }
        private bool _isEnableExpander { get; set; }
        public bool IsEnableExpander
        {
            get
            {
                return _isEnableExpander;
            }
            set
            {
                _isEnableExpander = value;
                RaisePropertyChanged(() => IsEnableExpander);
            }
        }

        public ObservableCollection<DefaultFormatModel> DefaultFormats { get; set; }

        public bool DisplayAmbiguousCoordsDlg { get; set; }

        private bool? dialogResult = null;
        public bool? DialogResult
        {
            get { return dialogResult; }
            set
            {
                dialogResult = value;
                RaisePropertyChanged(() => DialogResult);
            }
        }

        private ObservableCollection<Symbol> _symbols;
        public ObservableCollection<Symbol> Symbols
        {
            get
            {
                return _symbols;
            }
            set
            {
                _symbols = value;
                RaisePropertyChanged(() => Symbols);
            }
        }

        private ObservableCollection<ColorCollection> _popupDataCollections;

        public ObservableCollection<ColorCollection> PopupDataCollections
        {
            get { return _popupDataCollections; }
            set
            {
                _popupDataCollections = value;
                RaisePropertyChanged(() => PopupDataCollections);
            }
        }

        public string SearchString { get; set; }

        private bool showPlusForDirection;
        public bool ShowPlusForDirection
        {
            get { return showPlusForDirection; }
            set
            {
                showPlusForDirection = value;
                CoordinateBase.ShowPlus = value;
                RaisePropertyChanged(() => ShowPlusForDirection);
            }
        }

        private bool showHyphenForDirection;
        public bool ShowHyphenForDirection
        {
            get { return showHyphenForDirection; }
            set
            {
                showHyphenForDirection = value;
                CoordinateBase.ShowHyphen = value;
                RaisePropertyChanged(() => ShowHyphenForDirection);
            }
        }

        private bool showHemisphereIndicator;
        public bool ShowHemisphereIndicator
        {
            get { return showHemisphereIndicator; }
            set
            {
                showHemisphereIndicator = value;
                CoordinateBase.ShowHemisphere = value;
                RaisePropertyChanged(() => ShowHemisphereIndicator);
            }
        }

        private Visibility plusForDirectionVisibility;
        public Visibility PlusForDirectionVisibility
        {
            get { return plusForDirectionVisibility; }
            set
            {
                plusForDirectionVisibility = value;
                RaisePropertyChanged(() => PlusForDirectionVisibility);
            }
        }

        private Visibility hyphenForDirectionVisibility;
        public Visibility HyphenForDirectionVisibility
        {
            get { return hyphenForDirectionVisibility; }
            set
            {
                hyphenForDirectionVisibility = value;
                RaisePropertyChanged(() => HyphenForDirectionVisibility);
            }
        }

        private Visibility hemisphereIndicatorVisibility;
        public Visibility HemisphereIndicatorVisibility
        {
            get { return hemisphereIndicatorVisibility; }
            set
            {
                hemisphereIndicatorVisibility = value;
                RaisePropertyChanged(() => HemisphereIndicatorVisibility);
            }
        }
        #endregion

        /// <summary>
        /// Handler for when someone closes the dialog with the OK button
        /// </summary>
        /// <param name="obj"></param>
        private void OnOkButtonPressedCommand(object obj)
        {
            if (!IsValidFormat)
                return;

            CoordinateConversionLibraryConfig.AddInConfig.ShowPlusForDirection = ShowPlusForDirection;
            CoordinateConversionLibraryConfig.AddInConfig.ShowHyphenForDirection = ShowHyphenForDirection;
            CoordinateConversionLibraryConfig.AddInConfig.ShowHemisphereIndicator = ShowHemisphereIndicator;
            CoordinateConversionLibraryConfig.AddInConfig.DisplayCoordinateType = SelectedCoordinateType;
            CoordinateConversionLibraryConfig.AddInConfig.DisplayAmbiguousCoordsDlg = DisplayAmbiguousCoordsDlg;
            CoordinateConversionLibraryConfig.AddInConfig.FormatSelection = FormatSelection;
            if (FormatSelection == CoordinateConversionLibrary.Properties.Resources.CustomString)
            {
                CoordinateConversionLibraryConfig.AddInConfig.IsCustomFormat = true;
                CoordinateConversionLibraryConfig.AddInConfig.CategorySelection = CategorySelection;
            }
            else
            {
                CoordinateConversionLibraryConfig.AddInConfig.IsCustomFormat = false;
                CoordinateConversionLibraryConfig.AddInConfig.CategorySelection = CategorySelection;
            }
            CoordinateConversionLibraryConfig.AddInConfig.SaveConfiguration();

            CoordinateBase.InputCustomFormat = Format;
            // close dialog
            DialogResult = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        private void OnCancelButtonPressedCommand(object obj)
        {
            DialogResult = true;
        }

        private void OnSelectedButtonCommand(object obj)
        {
            IsPopUpOpen = !IsPopUpOpen;
        }

        private void OnSearchResultCommand(object obj)
        {
            if (!string.IsNullOrEmpty(SearchString))
                SymbolCollections = new ObservableCollection<Symbol>(AllSymbolCollections.Where(x => x.SymbolText.ToLower().Contains(SearchString.ToLower())));
            else
                SymbolCollections = new ObservableCollection<Symbol>(AllSymbolCollections);
        }

        private void OnCancelButtonCommand(object obj)
        {
            DialogResult = true;
        }

        private void OnApplyButtonCommand(object obj)
        {
            SelectedSymbolObject = SelectedStyleItem;
            SelectedColorObject = SelectedColor;
            DialogResult = true;
            SelectedSymbol = SelectedStyleItem;
            UpdateHighlightedGraphics(false, true);
        }

        private async Task<ObservableCollection<Symbol>> GetSymbolCollection()
        {
            return await QueuedTask.Run(() =>
            {
                var View3D = "ArcGIS 3D";
                var View2D = "ArcGIS 2D";
                Is3DMap = IsView3D();
                var mapType = Is3DMap ? View3D : View2D;
                if (AllSymbolCollection.Count == 0 || AllSymbolCollection.Where(x => x.Key == mapType).Count() == 0)
                {

                    SymbolCollections = new ObservableCollection<Symbol>();
                    var container = Project.Current.GetItems<StyleProjectItem>();

                    // Handle edge-case where project has no styles:
                    if ((container == null) || (container.Count() == 0))
                        return null;

                    var styleProjectItems = container.Where(style => style.Name == mapType).FirstOrDefault();
                    if (styleProjectItems == null)
                        return null;

                    var itemList = styleProjectItems.GetItems();
                    var symbolNames = itemList.Select(x => x.Name);
                    var lstSymbols = new List<StyleItemType>() { StyleItemType.PointSymbol };
                    foreach (var name in symbolNames)
                    {
                        var symbolQuery = lstSymbols.Select(x => styleProjectItems.SearchSymbols(x, name));
                        var listCollection = symbolQuery.SelectMany(x => x.Select(y => y));
                        foreach (var listItem in listCollection)
                        {
                            var image = listItem.PreviewImage;
                            var symbolImage = image as BitmapImage;
                            var wSource = symbolImage;
                            var wImage = new System.Windows.Controls.Image { Source = wSource };
                            Canvas.SetLeft(wImage, 20);
                            Canvas.SetTop(wImage, 20);
                            var variable = new Symbol() { SymbolImage = symbolImage, SymbolText = listItem.Name, SymbolItem = listItem };

                            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(() =>
                            {
                                SymbolCollections.Add(variable);
                            }));
                        }
                    }

                    AllSymbolCollection.Add(mapType, SymbolCollections);
                }
                else
                {
                    SymbolCollections = AllSymbolCollection.Where(x => x.Key == mapType).FirstOrDefault().Value;
                }
                return SymbolCollections;
            });
        }

        private void OnFormatSelectionChanged()
        {
            if (FormatSelection != CoordinateConversionLibrary.Properties.Resources.CustomString)
            {
                _format = GetFormatFromDefaults();
                UpdateSample();
                IsEnableExpander = false;
                FormatExpanded = false;
                CoordinateConversionLibraryConfig.AddInConfig.IsCustomFormat = false;
            }
            else
            {
                IsEnableExpander = true;
                FormatExpanded = true;
                CoordinateConversionLibraryConfig.AddInConfig.IsCustomFormat = true;
                RaisePropertyChanged(() => FormatExpanded);
            }
            ShowCheckBoxes();
        }
        private string GetFormatFromDefaults()
        {
            var item = DefaultFormats.FirstOrDefault(i => i.CType == GetCoordinateType());
            if (item == null)
                return CoordinateConversionLibrary.Properties.Resources.StringNoFormatFound;
            return item.DefaultNameFormatDictionary.Select(x => x.Value).FirstOrDefault();
        }
        private CoordinateType GetCoordinateType()
        {
            CoordinateType type;

            var selectedCoordinateType = Convert.ToString(SelectedCoordinateType);
            if (Enum.TryParse<CoordinateType>(selectedCoordinateType, out type))
                return type;

            return CoordinateType.Unknown;
        }
        private void UpdateSample()
        {
            var type = GetCoordinateType();

            switch (type)
            {
                case CoordinateType.DD:
                    var dd = new CoordinateDD();

                    if (ctdict.ContainsKey(CoordinateType.DD))
                    {
                        CoordinateDD.TryParse(ctdict[type], out dd);
                    }

                    Sample = dd.ToString(Format, new CoordinateDDFormatter());

                    break;
                case CoordinateType.DDM:
                    var ddm = new CoordinateDDM();

                    if (ctdict.ContainsKey(type))
                    {
                        CoordinateDDM.TryParse(ctdict[type], out ddm);
                    }

                    Sample = ddm.ToString(Format, new CoordinateDDMFormatter());
                    break;
                case CoordinateType.DMS:
                    var dms = new CoordinateDMS();

                    if (ctdict.ContainsKey(type))
                    {
                        CoordinateDMS.TryParse(ctdict[type], out dms);
                    }
                    Sample = dms.ToString(Format, new CoordinateDMSFormatter());
                    break;
                case CoordinateType.GARS:
                    var gars = new CoordinateGARS();

                    if (ctdict.ContainsKey(type))
                    {
                        CoordinateGARS.TryParse(ctdict[type], out gars);
                    }

                    Sample = gars.ToString(Format, new CoordinateGARSFormatter());
                    break;
                case CoordinateType.MGRS:
                    var mgrs = new CoordinateMGRS();

                    if (ctdict.ContainsKey(type))
                    {
                        CoordinateMGRS.TryParse(ctdict[type], out mgrs);
                    }

                    Sample = mgrs.ToString(Format, new CoordinateMGRSFormatter());
                    break;
                case CoordinateType.USNG:
                    var usng = new CoordinateUSNG();

                    if (ctdict.ContainsKey(type))
                    {
                        CoordinateUSNG.TryParse(ctdict[type], out usng);
                    }

                    Sample = usng.ToString(Format, new CoordinateMGRSFormatter());
                    break;
                case CoordinateType.UTM:
                    var utm = new CoordinateUTM();

                    if (ctdict.ContainsKey(type))
                    {
                        CoordinateUTM.TryParse(ctdict[type], out utm);
                    }

                    Sample = utm.ToString(Format, new CoordinateUTMFormatter());
                    break;
                default:
                    Sample = FormatList.FirstOrDefault();
                    break;
            }

            RaisePropertyChanged(() => Sample);
        }

        private void OnCategorySelectionChanged()
        {
            var selectedCoordinateType = Convert.ToString(SelectedCoordinateType);
            if (string.IsNullOrWhiteSpace(selectedCoordinateType))
                return;

            var list = GetFormatList(selectedCoordinateType);

            if (list == null)
                return;

            if (selectedCoordinateType != CoordinateType.Default.ToString())
                list.Add(CoordinateConversionLibrary.Properties.Resources.CustomString);

            FormatList = list;
            if ((
                (!FormatList.Contains(FormatSelection) || FormatSelection == CoordinateConversionLibrary.Properties.Resources.CustomString)
                && !CoordinateConversionLibraryConfig.AddInConfig.IsCustomFormat)
                )
            {
                // update format selection
                FormatSelection = FormatList.FirstOrDefault();
            }

            RaisePropertyChanged(() => FormatList);

            if (!CoordinateConversionLibraryConfig.AddInConfig.IsCustomFormat)
                Format = GetFormatFromDefaults();
            else
                Format = CoordinateBase.InputCustomFormat;
            RaisePropertyChanged(() => Format);
        }

        private ObservableCollection<string> GetFormatList(string CategorySelection)
        {
            var item = DefaultFormats.FirstOrDefault(i => i.CType == GetCoordinateType());

            if (item == null)
                return null;

            return new ObservableCollection<string>(item.DefaultNameFormatDictionary.Keys);
        }

        private void SelectCategory(CoordinateType coordinateType)
        {
            foreach (var item in CategoryList)
            {
                if (item == coordinateType.ToString())
                {
                    CategorySelection = item;
                    RaisePropertyChanged(() => SelectedCoordinateType);
                }
            }
        }

        private void SelectFormat(string format)
        {
            var defaultFormat = GetFormatSample(format);

            foreach (var item in FormatList)
            {
                if (item == defaultFormat)
                {
                    FormatSelection = item;
                    return;
                }
            }

            FormatSelection = FormatList.FirstOrDefault();
        }

        private string GetFormatSample(string format)
        {
            var cType = GetCoordinateType();
            var def = DefaultFormats.FirstOrDefault(i => i.CType == cType);

            if (def == null)
                return string.Empty;

            foreach (var item in def.DefaultNameFormatDictionary)
            {
                if (item.Value == format)
                {
                    return item.Key;
                }
            }

            return CoordinateConversionLibrary.Properties.Resources.CustomString;
        }

        private async void OnTabSelectionChanged()
        {
            if (SelectedTabIndex == 1)
            {
                ShowLoadingProcess = Visibility.Visible;
                var symbolCollections = await GetSymbolCollection();

                // Handle edge-case where project has no styles:
                if (symbolCollections == null)
                {
                    AllSymbolCollections = new ObservableCollection<Symbol>();
                    ShowLoadingProcess = Visibility.Collapsed;
                    return;
                }

                SymbolCollections = new ObservableCollection<Symbol>(symbolCollections);
                AllSymbolCollections = new ObservableCollection<Symbol>(symbolCollections);
                SelectedStyleItem = (SelectedSymbolObject == null) ? SymbolCollections.FirstOrDefault() : SelectedSymbolObject;
                SelectedSymbolText = SelectedStyleItem.SymbolText;
                ColorPickerCollection = new ObservableCollection<PropertyInfo>(typeof(System.Windows.Media.Brushes).GetProperties());
                SelectedColor = (SelectedColorObject == null) ? ColorPickerCollection.Where(x => x.Name == "Red").FirstOrDefault() : SelectedColorObject;
                SelectedBrush = (System.Windows.Media.Brush)SelectedColor.GetValue(null, null);
                ShowLoadingProcess = Visibility.Collapsed;
            }
        }

        private bool IsNotValidInput(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            var type = GetCoordinateType();
            if (type == CoordinateType.MGRS || type == CoordinateType.USNG || type == CoordinateType.UTM)
                return false;

            return (value.Contains("+") || value.Contains("-")
                || value.Contains("N") || value.Contains("S")
                || value.Contains("E") || value.Contains("W")
                || value.Contains("n") || value.Contains("s")
                || value.Contains("e") || value.Contains("w"));
        }

        private void ShowCheckBoxes()
        {
            if ((SelectedCoordinateType == CoordinateTypes.DD || SelectedCoordinateType == CoordinateTypes.DDM
                || SelectedCoordinateType == CoordinateTypes.DMS || SelectedCoordinateType == CoordinateTypes.Default)
                && FormatSelection == CoordinateConversionLibrary.Properties.Resources.CustomString)
            {
                PlusForDirectionVisibility = Visibility.Visible;
                HyphenForDirectionVisibility = Visibility.Visible;
                HemisphereIndicatorVisibility = Visibility.Visible;
                ShowHyphenForDirection = CoordinateConversionLibraryConfig.AddInConfig.ShowHyphenForDirection;
                ShowPlusForDirection = CoordinateConversionLibraryConfig.AddInConfig.ShowPlusForDirection;
                ShowHemisphereIndicator = CoordinateConversionLibraryConfig.AddInConfig.ShowHemisphereIndicator;
            }
            else
            {
                PlusForDirectionVisibility = Visibility.Collapsed;
                HyphenForDirectionVisibility = Visibility.Collapsed;
                HemisphereIndicatorVisibility = Visibility.Collapsed;
                ShowPlusForDirection = false;
                ShowHyphenForDirection = false;
                ShowHemisphereIndicator = false;
            }
        }
    }
}

