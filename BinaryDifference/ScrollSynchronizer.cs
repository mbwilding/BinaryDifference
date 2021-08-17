using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

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
			var scrollViewer = d as ScrollViewer;
			if (scrollViewer == null)
				return;

			var newVerticalGroupName = (e.NewValue == DependencyProperty.UnsetValue ? string.Empty : (string)e.NewValue);
			var oldVerticalGroupName = (e.NewValue == DependencyProperty.UnsetValue ? string.Empty : (string)e.OldValue);

			removeFromVerticalScrollGroup(oldVerticalGroupName, scrollViewer);
			addToVerticalScrollGroup(newVerticalGroupName, scrollViewer);

			var currentScrollSyncValue = readSyncTypeDPValue(d, ScrollSyncTypeProperty);
			if (currentScrollSyncValue == ScrollSyncType.None)
				d.SetValue(ScrollSyncTypeProperty, ScrollSyncType.Vertical);
			else if (currentScrollSyncValue == ScrollSyncType.Horizontal)
				d.SetValue(ScrollSyncTypeProperty, ScrollSyncType.Vertical);
		}

		private static void OnHorizontalScrollGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var scrollViewer = d as ScrollViewer;
			if (scrollViewer == null)
				return;

			var newHorizontalGroupName = (e.NewValue == DependencyProperty.UnsetValue ? string.Empty : (string)e.NewValue);
			var oldHorizontalGroupName = (e.NewValue == DependencyProperty.UnsetValue ? string.Empty : (string)e.OldValue);

			removeFromHorizontalScrollGroup(oldHorizontalGroupName, scrollViewer);
			addToHorizontalScrollGroup(newHorizontalGroupName, scrollViewer);

			var currentScrollSyncValue = readSyncTypeDPValue(d, ScrollSyncTypeProperty);
			if (currentScrollSyncValue == ScrollSyncType.None)
				d.SetValue(ScrollSyncTypeProperty, ScrollSyncType.Horizontal);
			else if (currentScrollSyncValue == ScrollSyncType.Vertical)
				d.SetValue(ScrollSyncTypeProperty, ScrollSyncType.Both);
		}

		private static void OnScrollSyncTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var scrollViewer = d as ScrollViewer;
			if (scrollViewer == null)
				return;

			var verticalGroupName = readStringDPValue(d, VerticalScrollGroupProperty);
			var horizontalGroupName = readStringDPValue(d, HorizontalScrollGroupProperty);

			var scrollSyncType = ScrollSyncType.None;
			try
			{
				scrollSyncType = (ScrollSyncType)e.NewValue;
			}
			catch { }

			switch (scrollSyncType)
			{
				case ScrollSyncType.None:
					if (!registeredScrollViewers.ContainsKey(scrollViewer))
						return;

					removeFromVerticalScrollGroup(verticalGroupName, scrollViewer);
					removeFromHorizontalScrollGroup(horizontalGroupName, scrollViewer);
					registeredScrollViewers.Remove(scrollViewer);

					break;
				case ScrollSyncType.Horizontal:
					removeFromVerticalScrollGroup(verticalGroupName, scrollViewer);
					addToHorizontalScrollGroup(horizontalGroupName, scrollViewer);

					if (registeredScrollViewers.ContainsKey(scrollViewer))
						registeredScrollViewers[scrollViewer] = ScrollSyncType.Horizontal;
					else
						registeredScrollViewers.Add(scrollViewer, ScrollSyncType.Horizontal);

					break;
				case ScrollSyncType.Vertical:
					removeFromHorizontalScrollGroup(horizontalGroupName, scrollViewer);
					addToVerticalScrollGroup(verticalGroupName, scrollViewer);

					if (registeredScrollViewers.ContainsKey(scrollViewer))
						registeredScrollViewers[scrollViewer] = ScrollSyncType.Vertical;
					else
						registeredScrollViewers.Add(scrollViewer, ScrollSyncType.Vertical);

					break;
				case ScrollSyncType.Both:
					if (registeredScrollViewers.ContainsKey(scrollViewer))
					{
						if (registeredScrollViewers[scrollViewer] == ScrollSyncType.Horizontal)
							addToVerticalScrollGroup(verticalGroupName, scrollViewer);
						else if (registeredScrollViewers[scrollViewer] == ScrollSyncType.Vertical)
							addToHorizontalScrollGroup(horizontalGroupName, scrollViewer);

						registeredScrollViewers[scrollViewer] = ScrollSyncType.Both;
					}
					else
					{
						addToHorizontalScrollGroup(horizontalGroupName, scrollViewer);
						addToVerticalScrollGroup(verticalGroupName, scrollViewer);

						registeredScrollViewers.Add(scrollViewer, ScrollSyncType.Both);
					}

					break;
			}
		}

		#endregion

		#endregion

		#region Variable(s)

		private static readonly Dictionary<string, OffSetContainer> verticalScrollGroups = new Dictionary<string, OffSetContainer>();
		private static readonly Dictionary<string, OffSetContainer> horizontalScrollGroups = new Dictionary<string, OffSetContainer>();
		private static readonly Dictionary<ScrollViewer, ScrollSyncType> registeredScrollViewers = new Dictionary<ScrollViewer, ScrollSyncType>();

		#endregion

		#region Method(s)

		private static void removeFromVerticalScrollGroup(string verticalGroupName, ScrollViewer scrollViewer)
		{
			if (verticalScrollGroups.ContainsKey(verticalGroupName))
			{
				verticalScrollGroups[verticalGroupName].ScrollViewers.Remove(scrollViewer);
				if (verticalScrollGroups[verticalGroupName].ScrollViewers.Count == 0)
					verticalScrollGroups.Remove(verticalGroupName);
			}

			scrollViewer.ScrollChanged -= ScrollViewer_VerticalScrollChanged;
		}

		private static void addToVerticalScrollGroup(string verticalGroupName, ScrollViewer scrollViewer)
		{
			if (verticalScrollGroups.ContainsKey(verticalGroupName))
			{
				scrollViewer.ScrollToVerticalOffset(verticalScrollGroups[verticalGroupName].Offset);
				verticalScrollGroups[verticalGroupName].ScrollViewers.Add(scrollViewer);
			}
			else
			{
				verticalScrollGroups.Add(verticalGroupName, new OffSetContainer { ScrollViewers = new List<ScrollViewer> { scrollViewer }, Offset = scrollViewer.VerticalOffset });
			}

			scrollViewer.ScrollChanged += ScrollViewer_VerticalScrollChanged;
		}

		private static void removeFromHorizontalScrollGroup(string horizontalGroupName, ScrollViewer scrollViewer)
		{
			if (horizontalScrollGroups.ContainsKey(horizontalGroupName))
			{
				horizontalScrollGroups[horizontalGroupName].ScrollViewers.Remove(scrollViewer);
				if (horizontalScrollGroups[horizontalGroupName].ScrollViewers.Count == 0)
					horizontalScrollGroups.Remove(horizontalGroupName);
			}

			scrollViewer.ScrollChanged -= ScrollViewer_HorizontalScrollChanged;
		}

		private static void addToHorizontalScrollGroup(string horizontalGroupName, ScrollViewer scrollViewer)
		{
			if (horizontalScrollGroups.ContainsKey(horizontalGroupName))
			{
				scrollViewer.ScrollToHorizontalOffset(horizontalScrollGroups[horizontalGroupName].Offset);
				horizontalScrollGroups[horizontalGroupName].ScrollViewers.Add(scrollViewer);
			}
			else
			{
				horizontalScrollGroups.Add(horizontalGroupName, new OffSetContainer { ScrollViewers = new List<ScrollViewer> { scrollViewer }, Offset = scrollViewer.HorizontalOffset });
			}

			scrollViewer.ScrollChanged += ScrollViewer_HorizontalScrollChanged;
		}

		private static string readStringDPValue(DependencyObject d, DependencyProperty dp)
		{
			var value = d.ReadLocalValue(dp);
			return (value == DependencyProperty.UnsetValue ? string.Empty : value.ToString());
		}

		private static ScrollSyncType readSyncTypeDPValue(DependencyObject d, DependencyProperty dp)
		{
			var value = d.ReadLocalValue(dp);
			return (value == DependencyProperty.UnsetValue ? ScrollSyncType.None : (ScrollSyncType)value);
		}

		#endregion

		#region Event Handler(s)

		private static void ScrollViewer_VerticalScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			var changedScrollViewer = sender as ScrollViewer;
			if (changedScrollViewer == null)
				return;

			if (e.VerticalChange == 0)
				return;

			var verticalScrollGroup = readStringDPValue(sender as DependencyObject, VerticalScrollGroupProperty);
			if (!verticalScrollGroups.ContainsKey(verticalScrollGroup))
				return;

			verticalScrollGroups[verticalScrollGroup].Offset = changedScrollViewer.VerticalOffset;

			foreach (var scrollViewer in verticalScrollGroups[verticalScrollGroup].ScrollViewers)
			{
				if (scrollViewer.VerticalOffset == changedScrollViewer.VerticalOffset)
					continue;

				scrollViewer.ScrollToVerticalOffset(changedScrollViewer.VerticalOffset);
			}
		}

		private static void ScrollViewer_HorizontalScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			var changedScrollViewer = sender as ScrollViewer;
			if (changedScrollViewer == null)
				return;

			if (e.HorizontalChange == 0)
				return;

			var horizontalScrollGroup = readStringDPValue(sender as DependencyObject, HorizontalScrollGroupProperty);
			if (!horizontalScrollGroups.ContainsKey(horizontalScrollGroup))
				return;

			horizontalScrollGroups[horizontalScrollGroup].Offset = changedScrollViewer.HorizontalOffset;

			foreach (var scrollViewer in horizontalScrollGroups[horizontalScrollGroup].ScrollViewers)
			{
				if (scrollViewer.HorizontalOffset == changedScrollViewer.HorizontalOffset)
					continue;

				scrollViewer.ScrollToHorizontalOffset(changedScrollViewer.HorizontalOffset);
			}
		}

		#endregion

		#region Class(es)

		private class OffSetContainer
		{
			public double Offset { get; set; }
			public List<ScrollViewer> ScrollViewers { get; set; }
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
