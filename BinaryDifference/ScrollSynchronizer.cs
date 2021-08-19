using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
// ReSharper disable UnusedMember.Global

// ReSharper disable once CheckNamespace
namespace ScrollViewerSynchronization.Core
{
	public sealed class ScrollSynchronizer
	{
		#region Constant(s)

		private const string VerticalScrollGroupPropertyName = "VerticalScrollGroup";
		private const string HorizontalScrollGroupPropertyName = "HorizontalScrollGroup";
		private const string ScrollSyncTypePropertyName = "ScrollSyncType";

		#endregion

		#region Dependency Propert(y/ies)

		#region Declaration(s)

		public static readonly DependencyProperty HorizontalScrollGroupProperty =
			DependencyProperty.RegisterAttached(HorizontalScrollGroupPropertyName, typeof(string), typeof(ScrollSynchronizer), new PropertyMetadata(string.Empty, OnHorizontalScrollGroupChanged));
		public static readonly DependencyProperty VerticalScrollGroupProperty =
			DependencyProperty.RegisterAttached(VerticalScrollGroupPropertyName, typeof(string), typeof(ScrollSynchronizer), new PropertyMetadata(string.Empty, OnVerticalScrollGroupChanged));
		public static readonly DependencyProperty ScrollSyncTypeProperty =
			DependencyProperty.RegisterAttached(ScrollSyncTypePropertyName, typeof(ScrollSyncType), typeof(ScrollSynchronizer), new PropertyMetadata(ScrollSyncType.None, OnScrollSyncTypeChanged));

		#endregion

		#region Getter(s)/Setter(s)

		public static void SetVerticalScrollGroup(DependencyObject obj, string verticalScrollGroup)
		{
			obj.SetValue(VerticalScrollGroupProperty, verticalScrollGroup);
		}

		public static string GetVerticalScrollGroup(DependencyObject obj)
		{
			return (string)obj.GetValue(VerticalScrollGroupProperty);
		}

		public static void SetHorizontalScrollGroup(DependencyObject obj, string horizontalScrollGroup)
		{
			obj.SetValue(HorizontalScrollGroupProperty, horizontalScrollGroup);
		}

		public static string GetHorizontalScrollGroup(DependencyObject obj)
		{
			return (string)obj.GetValue(HorizontalScrollGroupProperty);
		}

		public static void SetScrollSyncType(DependencyObject obj, ScrollSyncType scrollSyncType)
		{
			obj.SetValue(ScrollSyncTypeProperty, scrollSyncType);
		}

		public static ScrollSyncType GetScrollSyncType(DependencyObject obj)
		{
			return (ScrollSyncType)obj.GetValue(ScrollSyncTypeProperty);
		}

		#endregion

		#region Event Handler(s)

		private static void OnVerticalScrollGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
            if (d is not ScrollViewer scrollViewer)
                return;

			var newVerticalGroupName = (e.NewValue == DependencyProperty.UnsetValue ? string.Empty : (string)e.NewValue);
			var oldVerticalGroupName = (e.NewValue == DependencyProperty.UnsetValue ? string.Empty : (string)e.OldValue);

			RemoveFromVerticalScrollGroup(oldVerticalGroupName, scrollViewer);
			AddToVerticalScrollGroup(newVerticalGroupName, scrollViewer);

			var currentScrollSyncValue = ReadSyncTypeDpValue(d, ScrollSyncTypeProperty);
			if (currentScrollSyncValue == ScrollSyncType.None)
				d.SetValue(ScrollSyncTypeProperty, ScrollSyncType.Vertical);
			else if (currentScrollSyncValue == ScrollSyncType.Horizontal)
				d.SetValue(ScrollSyncTypeProperty, ScrollSyncType.Vertical);
		}

		private static void OnHorizontalScrollGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
            if (d is not ScrollViewer scrollViewer)
                return;

			var newHorizontalGroupName = (e.NewValue == DependencyProperty.UnsetValue ? string.Empty : (string)e.NewValue);
			var oldHorizontalGroupName = (e.NewValue == DependencyProperty.UnsetValue ? string.Empty : (string)e.OldValue);

			RemoveFromHorizontalScrollGroup(oldHorizontalGroupName, scrollViewer);
			AddToHorizontalScrollGroup(newHorizontalGroupName, scrollViewer);

			var currentScrollSyncValue = ReadSyncTypeDpValue(d, ScrollSyncTypeProperty);
			if (currentScrollSyncValue == ScrollSyncType.None)
				d.SetValue(ScrollSyncTypeProperty, ScrollSyncType.Horizontal);
			else if (currentScrollSyncValue == ScrollSyncType.Vertical)
				d.SetValue(ScrollSyncTypeProperty, ScrollSyncType.Both);
		}

		private static void OnScrollSyncTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
            if (d is not ScrollViewer scrollViewer)
                return;

			var verticalGroupName = ReadStringDpValue(d, VerticalScrollGroupProperty);
			var horizontalGroupName = ReadStringDpValue(d, HorizontalScrollGroupProperty);

			var scrollSyncType = ScrollSyncType.None;
			try
			{
				scrollSyncType = (ScrollSyncType)e.NewValue;
			}
            catch
            {
                // ignored
            }

            switch (scrollSyncType)
			{
				case ScrollSyncType.None:
					if (!RegisteredScrollViewers.ContainsKey(scrollViewer))
						return;

					RemoveFromVerticalScrollGroup(verticalGroupName, scrollViewer);
					RemoveFromHorizontalScrollGroup(horizontalGroupName, scrollViewer);
					RegisteredScrollViewers.Remove(scrollViewer);

					break;
				case ScrollSyncType.Horizontal:
					RemoveFromVerticalScrollGroup(verticalGroupName, scrollViewer);
					AddToHorizontalScrollGroup(horizontalGroupName, scrollViewer);

					if (RegisteredScrollViewers.ContainsKey(scrollViewer))
						RegisteredScrollViewers[scrollViewer] = ScrollSyncType.Horizontal;
					else
						RegisteredScrollViewers.Add(scrollViewer, ScrollSyncType.Horizontal);

					break;
				case ScrollSyncType.Vertical:
					RemoveFromHorizontalScrollGroup(horizontalGroupName, scrollViewer);
					AddToVerticalScrollGroup(verticalGroupName, scrollViewer);

					if (RegisteredScrollViewers.ContainsKey(scrollViewer))
						RegisteredScrollViewers[scrollViewer] = ScrollSyncType.Vertical;
					else
						RegisteredScrollViewers.Add(scrollViewer, ScrollSyncType.Vertical);

					break;
				case ScrollSyncType.Both:
					if (RegisteredScrollViewers.ContainsKey(scrollViewer))
					{
						if (RegisteredScrollViewers[scrollViewer] == ScrollSyncType.Horizontal)
							AddToVerticalScrollGroup(verticalGroupName, scrollViewer);
						else if (RegisteredScrollViewers[scrollViewer] == ScrollSyncType.Vertical)
							AddToHorizontalScrollGroup(horizontalGroupName, scrollViewer);

						RegisteredScrollViewers[scrollViewer] = ScrollSyncType.Both;
					}
					else
					{
						AddToHorizontalScrollGroup(horizontalGroupName, scrollViewer);
						AddToVerticalScrollGroup(verticalGroupName, scrollViewer);

						RegisteredScrollViewers.Add(scrollViewer, ScrollSyncType.Both);
					}

					break;
			}
		}

		#endregion

		#endregion

		#region Variable(s)

		private static readonly Dictionary<string, OffSetContainer> VerticalScrollGroups = new();
		private static readonly Dictionary<string, OffSetContainer> HorizontalScrollGroups = new();
		private static readonly Dictionary<ScrollViewer, ScrollSyncType> RegisteredScrollViewers = new();

		#endregion

		#region Method(s)

		private static void RemoveFromVerticalScrollGroup(string verticalGroupName, ScrollViewer scrollViewer)
		{
			if (VerticalScrollGroups.ContainsKey(verticalGroupName))
			{
				VerticalScrollGroups[verticalGroupName].ScrollViewers.Remove(scrollViewer);
				if (VerticalScrollGroups[verticalGroupName].ScrollViewers.Count == 0)
					VerticalScrollGroups.Remove(verticalGroupName);
			}

			scrollViewer.ScrollChanged -= ScrollViewer_VerticalScrollChanged;
		}

		private static void AddToVerticalScrollGroup(string verticalGroupName, ScrollViewer scrollViewer)
		{
			if (VerticalScrollGroups.ContainsKey(verticalGroupName))
			{
				scrollViewer.ScrollToVerticalOffset(VerticalScrollGroups[verticalGroupName].Offset);
				VerticalScrollGroups[verticalGroupName].ScrollViewers.Add(scrollViewer);
			}
			else
			{
				VerticalScrollGroups.Add(verticalGroupName, new OffSetContainer { ScrollViewers = new List<ScrollViewer> { scrollViewer }, Offset = scrollViewer.VerticalOffset });
			}

			scrollViewer.ScrollChanged += ScrollViewer_VerticalScrollChanged;
		}

		private static void RemoveFromHorizontalScrollGroup(string horizontalGroupName, ScrollViewer scrollViewer)
		{
			if (HorizontalScrollGroups.ContainsKey(horizontalGroupName))
			{
				HorizontalScrollGroups[horizontalGroupName].ScrollViewers.Remove(scrollViewer);
				if (HorizontalScrollGroups[horizontalGroupName].ScrollViewers.Count == 0)
					HorizontalScrollGroups.Remove(horizontalGroupName);
			}

			scrollViewer.ScrollChanged -= ScrollViewer_HorizontalScrollChanged;
		}

		private static void AddToHorizontalScrollGroup(string horizontalGroupName, ScrollViewer scrollViewer)
		{
			if (HorizontalScrollGroups.ContainsKey(horizontalGroupName))
			{
				scrollViewer.ScrollToHorizontalOffset(HorizontalScrollGroups[horizontalGroupName].Offset);
				HorizontalScrollGroups[horizontalGroupName].ScrollViewers.Add(scrollViewer);
			}
			else
			{
				HorizontalScrollGroups.Add(horizontalGroupName, new OffSetContainer { ScrollViewers = new List<ScrollViewer> { scrollViewer }, Offset = scrollViewer.HorizontalOffset });
			}

			scrollViewer.ScrollChanged += ScrollViewer_HorizontalScrollChanged;
		}

		private static string ReadStringDpValue(DependencyObject d, DependencyProperty dp)
		{
			var value = d.ReadLocalValue(dp);
			return (value == DependencyProperty.UnsetValue ? string.Empty : value.ToString());
		}

		private static ScrollSyncType ReadSyncTypeDpValue(DependencyObject d, DependencyProperty dp)
		{
			var value = d.ReadLocalValue(dp);
			return (value == DependencyProperty.UnsetValue ? ScrollSyncType.None : (ScrollSyncType)value);
		}

		#endregion

		#region Event Handler(s)

		private static void ScrollViewer_VerticalScrollChanged(object sender, ScrollChangedEventArgs e)
		{
            if (sender is not ScrollViewer changedScrollViewer)
                return;

            if (e.VerticalChange == 0)
				return;

			var verticalScrollGroup = ReadStringDpValue(changedScrollViewer, VerticalScrollGroupProperty);
			if (!VerticalScrollGroups.ContainsKey(verticalScrollGroup))
				return;

			VerticalScrollGroups[verticalScrollGroup].Offset = changedScrollViewer.VerticalOffset;

			foreach (var scrollViewer in VerticalScrollGroups[verticalScrollGroup].ScrollViewers)
			{
				if (Math.Abs(scrollViewer.VerticalOffset - changedScrollViewer.VerticalOffset) < 0)
					continue;

				scrollViewer.ScrollToVerticalOffset(changedScrollViewer.VerticalOffset);
			}
		}

		private static void ScrollViewer_HorizontalScrollChanged(object sender, ScrollChangedEventArgs e)
		{
            if (sender is not ScrollViewer changedScrollViewer)
                return;

            if (e.HorizontalChange == 0)
				return;

			var horizontalScrollGroup = ReadStringDpValue(changedScrollViewer, HorizontalScrollGroupProperty);
			if (!HorizontalScrollGroups.ContainsKey(horizontalScrollGroup))
				return;

			HorizontalScrollGroups[horizontalScrollGroup].Offset = changedScrollViewer.HorizontalOffset;

			foreach (var scrollViewer in HorizontalScrollGroups[horizontalScrollGroup].ScrollViewers)
			{
				if (Math.Abs(scrollViewer.HorizontalOffset - changedScrollViewer.HorizontalOffset) < 0)
					continue;

				scrollViewer.ScrollToHorizontalOffset(changedScrollViewer.HorizontalOffset);
			}
		}

		#endregion

		#region Class(es)

		private class OffSetContainer
		{
			public double Offset { get; set; }
			public List<ScrollViewer> ScrollViewers { get; init; }
		}

		#endregion
	}

	public enum ScrollSyncType
	{
		Both,
		Horizontal,
		Vertical,
		None
	}
}
