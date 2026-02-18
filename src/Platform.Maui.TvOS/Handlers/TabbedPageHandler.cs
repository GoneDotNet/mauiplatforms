using CoreGraphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui.Platform.TvOS.Handlers;

public class TabbedContainerView : TvOSContainerView
{
    readonly UIStackView _tabBar;
    readonly UIView _contentArea;
    readonly List<UIButton> _tabButtons = new();

    UIView? _currentPageView;
    int _selectedIndex = -1;

    public Action<int>? OnTabSelected { get; set; }
    public Action<CGRect>? OnContentLayout { get; set; }

    public TabbedContainerView()
    {
        _tabBar = new UIStackView
        {
            Axis = UILayoutConstraintAxis.Horizontal,
            Distribution = UIStackViewDistribution.FillEqually,
            Spacing = 8,
            TranslatesAutoresizingMaskIntoConstraints = false,
        };

        _contentArea = new UIView
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            BackgroundColor = UIColor.Clear,
        };

        AddSubview(_tabBar);
        AddSubview(_contentArea);

        NSLayoutConstraint.ActivateConstraints(new[]
        {
            _tabBar.TopAnchor.ConstraintEqualTo(TopAnchor, 20),
            _tabBar.LeadingAnchor.ConstraintEqualTo(LeadingAnchor, 60),
            _tabBar.TrailingAnchor.ConstraintEqualTo(TrailingAnchor, -60),
            _tabBar.HeightAnchor.ConstraintEqualTo(80),

            _contentArea.TopAnchor.ConstraintEqualTo(_tabBar.BottomAnchor, 10),
            _contentArea.LeadingAnchor.ConstraintEqualTo(LeadingAnchor),
            _contentArea.TrailingAnchor.ConstraintEqualTo(TrailingAnchor),
            _contentArea.BottomAnchor.ConstraintEqualTo(BottomAnchor),
        });
    }

    public void SetTabs(IList<string> titles)
    {
        foreach (var btn in _tabButtons)
            btn.RemoveFromSuperview();
        _tabButtons.Clear();

        for (int i = 0; i < titles.Count; i++)
        {
            var index = i;
            var btn = new UIButton(UIButtonType.System);
            btn.SetTitle(titles[i], UIControlState.Normal);
            btn.TitleLabel!.Font = UIFont.BoldSystemFontOfSize(28);
            btn.PrimaryActionTriggered += (s, e) => OnTabSelected?.Invoke(index);
            _tabButtons.Add(btn);
            _tabBar.AddArrangedSubview(btn);
        }

        UpdateTabAppearance();
    }

    public void SelectTab(int index)
    {
        if (index == _selectedIndex)
            return;

        _selectedIndex = index;
        UpdateTabAppearance();
    }

    void UpdateTabAppearance()
    {
        for (int i = 0; i < _tabButtons.Count; i++)
        {
            var btn = _tabButtons[i];
            if (i == _selectedIndex)
            {
                btn.SetTitleColor(UIColor.White, UIControlState.Normal);
                btn.BackgroundColor = UIColor.FromRGBA(74, 144, 226, 255);
                btn.Layer.CornerRadius = 12;
            }
            else
            {
                btn.SetTitleColor(UIColor.LightGray, UIControlState.Normal);
                btn.BackgroundColor = UIColor.FromRGBA(42, 42, 74, 255);
                btn.Layer.CornerRadius = 12;
            }
        }
    }

    public void ShowContent(UIView view)
    {
        _currentPageView?.RemoveFromSuperview();
        _currentPageView = view;

        view.Frame = _contentArea.Bounds;
        view.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
        _contentArea.AddSubview(view);
    }

    public override void LayoutSubviews()
    {
        base.LayoutSubviews();
        if (_currentPageView != null)
        {
            _currentPageView.Frame = _contentArea.Bounds;
            OnContentLayout?.Invoke(_contentArea.Bounds);
        }
    }
}

public partial class TabbedPageHandler : TvOSViewHandler<ITabbedView, TabbedContainerView>
{
    public static readonly IPropertyMapper<ITabbedView, TabbedPageHandler> Mapper =
        new PropertyMapper<ITabbedView, TabbedPageHandler>(ViewMapper);

    public TabbedPageHandler() : base(Mapper)
    {
    }

    TabbedPage? TabbedPage => VirtualView as TabbedPage;

    protected override TabbedContainerView CreatePlatformView()
    {
        var view = new TabbedContainerView();
        view.OnTabSelected = OnTabSelected;
        view.OnContentLayout = OnContentLayout;
        return view;
    }

    protected override void ConnectHandler(TabbedContainerView platformView)
    {
        base.ConnectHandler(platformView);

        if (TabbedPage != null)
        {
            TabbedPage.PagesChanged += OnPagesChanged;
            SetupTabs();
        }
    }

    protected override void DisconnectHandler(TabbedContainerView platformView)
    {
        if (TabbedPage != null)
            TabbedPage.PagesChanged -= OnPagesChanged;

        base.DisconnectHandler(platformView);
    }

    void OnPagesChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        SetupTabs();
    }

    void SetupTabs()
    {
        if (TabbedPage == null)
            return;

        var titles = new List<string>();
        foreach (var page in TabbedPage.Children)
            titles.Add(page.Title ?? "Tab");

        PlatformView.SetTabs(titles);

        if (TabbedPage.Children.Count > 0)
            SelectPage(0);
    }

    void OnTabSelected(int index)
    {
        SelectPage(index);
    }

    void SelectPage(int index)
    {
        if (TabbedPage == null || index < 0 || index >= TabbedPage.Children.Count || MauiContext == null)
            return;

        TabbedPage.CurrentPage = TabbedPage.Children[index];
        PlatformView.SelectTab(index);

        var page = TabbedPage.Children[index];
        var platformView = ((IView)page).ToTvOSPlatform(MauiContext);
        PlatformView.ShowContent(platformView);
    }

    void OnContentLayout(CGRect bounds)
    {
        if (TabbedPage?.CurrentPage == null || bounds.Width <= 0 || bounds.Height <= 0)
            return;

        var currentPage = (IView)TabbedPage.CurrentPage;
        currentPage.Measure((double)bounds.Width, (double)bounds.Height);
        currentPage.Arrange(new Rect(0, 0, (double)bounds.Width, (double)bounds.Height));
    }
}
