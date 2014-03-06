using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using System.Linq;

namespace ListViewFilter
{
	[Activity (Label = "ListViewFilter", MainLauncher = true)]
	public class MainActivity : Activity
	{
		Button _button;
		ListView _listview;
		EditText _filterText;
		FilterableAdapter _adapter;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			_button = FindViewById<Button> (Resource.Id.dosearch);
			_listview = FindViewById<ListView> (Resource.Id.list);
			_filterText = FindViewById<EditText> (Resource.Id.search);
			_adapter = new FilterableAdapter (this, Android.Resource.Layout.SimpleListItem1, GetItems());
			_listview.Adapter = _adapter;

			_button.Click += delegate {
				// filter the adapter here
				_adapter.Filter.InvokeFilter(_filterText.Text);
			};

			_filterText.TextChanged += (object sender, Android.Text.TextChangedEventArgs e) => {
				// filter on text changed
				var searchTerm = _filterText.Text;
				if (String.IsNullOrEmpty(searchTerm)) {
					_adapter.ResetSearch();
				} else {
					_adapter.Filter.InvokeFilter(searchTerm);
				}
			};
		}

		string[] GetItems ()
		{
			string[] names = new string[] { "Ljubljana	280,000", "Maribor	130,000", "Celje	56,000", "Kranj	52,000", "Novo Mesto	48,000", "Nova Gorica	45,000", "Koper/Capodistria	42,000", "Velenje	32,000", "Domžale	30,000", "Ptuj	25,000", "Murska Sobota	22,000", "Jesenice	20,000", "Trbovlje	16,000", "Kamnik	15,000", "Škofja Loka	12,000", "Izola/Isola	11,000" };
			Random r = new Random ();
			List<string> res = new List<string> ();
			for (int i = 0; i < 1000; i++) {
				res.Add (names [r.Next (names.Length)]);
			}
			return names.ToArray ();
		}
	}

	public class FilterableAdapter : ArrayAdapter, IFilterable
	{
		LayoutInflater inflater;
		Filter filter;
		Activity context;
		public string[] AllItems;
		public string[] MatchItems;

		public FilterableAdapter (Activity context, int txtViewResourceId, string[] items) : base(context, txtViewResourceId, items)
		{
			inflater = context.LayoutInflater;
			filter = new SuggestionsFilter(this);
			AllItems = items;
			MatchItems = items;
		}

		public override int Count {
			get {
				return MatchItems.Length;
			}
		}

		public override Java.Lang.Object GetItem (int position)
		{
			return MatchItems[position];
		}

		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			View view = convertView;
			if (view == null)
				view = inflater.Inflate(Android.Resource.Layout.SimpleDropDownItem1Line, null);

			view.FindViewById<TextView>(Android.Resource.Id.Text1).Text = MatchItems[position];

			return view;
		}

		public override Filter Filter {
			get {
				return filter;
			}
		}

		public void ResetSearch() {
			MatchItems = AllItems;
			NotifyDataSetChanged ();
		}

		class SuggestionsFilter : Filter
		{
			readonly FilterableAdapter _adapter;

			public SuggestionsFilter (FilterableAdapter adapter) : base() {
				_adapter = adapter;
			}

			protected override Filter.FilterResults PerformFiltering (Java.Lang.ICharSequence constraint)
			{
				FilterResults results = new FilterResults();
				if (!String.IsNullOrEmpty (constraint.ToString ())) {
					var searchFor = constraint.ToString ();
					Console.WriteLine ("searchFor:" + searchFor);
					var matchList = new List<string> ();

					var matches = 
						from i in _adapter.AllItems
						where i.IndexOf (searchFor, StringComparison.InvariantCultureIgnoreCase) >= 0
						select i;

					foreach (var match in matches) {
						matchList.Add (match);
					}

					_adapter.MatchItems = matchList.ToArray ();
					Console.WriteLine ("resultCount:" + matchList.Count);

					Java.Lang.Object[] matchObjects;
					matchObjects = new Java.Lang.Object[matchList.Count];
					for (int i = 0; i < matchList.Count; i++) {
						matchObjects [i] = new Java.Lang.String (matchList [i]);
					}

					results.Values = matchObjects;
					results.Count = matchList.Count;
				} else {
					_adapter.ResetSearch ();
				}
				return results;
			}

			protected override void PublishResults (Java.Lang.ICharSequence constraint, Filter.FilterResults results)
			{
				_adapter.NotifyDataSetChanged();
			}
		}
	}
}


