using System.Collections;
using System.Collections.Specialized;
using CoreGraphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using AppKit;

namespace Microsoft.Maui.Platform.MacOS.Handlers;

public partial class CollectionViewHandler : MacOSViewHandler<CollectionView, NSScrollView>
{
    public static readonly IPropertyMapper<CollectionView, CollectionViewHandler> Mapper =
        new PropertyMapper<CollectionView, CollectionViewHandler>(ViewMapper)
        {
            [nameof(ItemsView.ItemsSource)] = MapItemsSource,
            [nameof(ItemsView.ItemTemplate)] = MapItemTemplate,
        };

    FlippedDocumentView? _documentView;
    MacOSContainerView? _itemsContainer;
    INotifyCollectionChanged? _observableSource;

    public CollectionViewHandler() : base(Mapper)
    {
    }

    protected override NSScrollView CreatePlatformView()
    {
        var scrollView = new NSScrollView
        {
            HasVerticalScroller = true,
            HasHorizontalScroller = false,
            AutohidesScrollers = true,
            DrawsBackground = false,
        };

        _documentView = new FlippedDocumentView();
        _itemsContainer = new MacOSContainerView();
        _documentView.AddSubview(_itemsContainer);
        scrollView.DocumentView = _documentView;

        return scrollView;
    }

    protected override void DisconnectHandler(NSScrollView platformView)
    {
        UnsubscribeCollection();
        base.DisconnectHandler(platformView);
    }

    public override void PlatformArrange(Rect rect)
    {
        base.PlatformArrange(rect);
        LayoutItems(rect);
    }

    void LayoutItems(Rect rect)
    {
        if (_itemsContainer == null || _documentView == null)
            return;

        var subviews = _itemsContainer.Subviews;
        if (subviews.Length == 0)
            return;

        nfloat y = 0;
        var width = rect.Width;

        foreach (var subview in subviews)
        {
            var fittingSize = subview is MacOSContainerView container
                ? container.SizeThatFits(new CGSize(width, nfloat.MaxValue))
                : subview.IntrinsicContentSize;

            var height = fittingSize.Height > 0 && fittingSize.Height < 10000
                ? fittingSize.Height
                : 44;

            subview.Frame = new CGRect(0, y, width, height);
            y += height;
        }

        _itemsContainer.Frame = new CGRect(0, 0, width, y);
        _documentView.Frame = new CGRect(0, 0, width, y);
    }

    public static void MapItemsSource(CollectionViewHandler handler, CollectionView view)
    {
        handler.ReloadItems();
    }

    public static void MapItemTemplate(CollectionViewHandler handler, CollectionView view)
    {
        handler.ReloadItems();
    }

    void UnsubscribeCollection()
    {
        if (_observableSource != null)
        {
            _observableSource.CollectionChanged -= OnCollectionChanged;
            _observableSource = null;
        }
    }

    void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ReloadItems();
    }

    void ReloadItems()
    {
        if (_itemsContainer == null || MauiContext == null)
            return;

        UnsubscribeCollection();

        foreach (var subview in _itemsContainer.Subviews)
            subview.RemoveFromSuperview();

        var itemsSource = VirtualView?.ItemsSource;
        if (itemsSource == null)
            return;

        if (itemsSource is INotifyCollectionChanged observable)
        {
            _observableSource = observable;
            _observableSource.CollectionChanged += OnCollectionChanged;
        }

        var template = VirtualView?.ItemTemplate;

        foreach (var item in itemsSource)
        {
            var view = CreateItemView(item, template);
            if (view != null)
            {
                var platformView = view.ToMacOSPlatform(MauiContext);
                _itemsContainer.AddSubview(platformView);
            }
        }

        if (PlatformView.Frame.Width > 0)
            LayoutItems(new Rect(0, 0, PlatformView.Frame.Width, PlatformView.Frame.Height));
    }

    static IView? CreateItemView(object item, DataTemplate? template)
    {
        if (template != null)
        {
            var content = template.CreateContent();
            if (content is View view)
            {
                view.BindingContext = item;
                return view;
            }
        }

        // Default: create a label with the item's ToString()
        return new Label { Text = item?.ToString() ?? string.Empty };
    }
}
