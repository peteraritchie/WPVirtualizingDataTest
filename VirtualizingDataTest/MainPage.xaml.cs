using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using System.Diagnostics;

namespace VirtualizingDataTest
{
	public partial class MainPage : PhoneApplicationPage
	{
		public MainPage()
		{
			InitializeComponent();
		}

		private void Start_Click(object sender, RoutedEventArgs e)
		{
			TheList.ItemsSource = new VirtualizedDataSource<string>(10000, 25, GetPageOfItemsAsync, GetPlaceholder);
		}

		private static string GetPlaceholder(int index)
		{
			return string.Format("getting item {0}...", index);
		}

		private Task<IEnumerable<string>> GetPageOfItemsAsync(int f, int p)
		{
			return Task<IEnumerable<string>>.Factory.StartNew(() => GetPageOfItems(f, p));
		}

		private IEnumerable<string> GetPageOfItems(int firstItemInPage, int pageSize)
		{
			var newItems = new List<string>(pageSize);
			for (var i = firstItemInPage; i < firstItemInPage + pageSize; ++i)
			{
				newItems.Add(string.Format("ITEM {0}", i));
			}
			return newItems;
		}

		private void Jump_Click(object sender, RoutedEventArgs e)
		{
			int index;
			if (!int.TryParse(NewIndex.Text, out index)) return;
			Debug.WriteLine("Jump to " + index);
			TheList.SelectedIndex = index;
			TheList.ScrollIntoView(TheList.SelectedItem);
		}

		private void NewIndex_TextChanged(object sender, TextChangedEventArgs e)
		{
			int x;
			Jump.IsEnabled = int.TryParse(NewIndex.Text, out x);
		}
	}
}