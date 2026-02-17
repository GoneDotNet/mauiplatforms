using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Sample.Pages;

public class CollectionViewPage : ContentPage
{
	public CollectionViewPage()
	{
		Title = "CollectionView";

		var tabs = new HorizontalStackLayout { Spacing = 0 };
		var contentArea = new VerticalStackLayout();

		var pages = new (string title, Func<View> builder)[]
		{
			("Simple List", BuildSimpleList),
			("Templated", BuildTemplatedList),
			("Multi-Select", BuildMultiSelectList),
		};

		Button? activeTab = null;
		foreach (var (title, builder) in pages)
		{
			var btn = new Button
			{
				Text = title,
				FontSize = 13,
				BackgroundColor = Colors.Transparent,
				TextColor = Colors.Gray,
				Padding = new Thickness(16, 8),
			};
			var capturedBuilder = builder;
			btn.Clicked += (s, e) =>
			{
				if (activeTab != null)
				{
					activeTab.TextColor = Colors.Gray;
					activeTab.BackgroundColor = Colors.Transparent;
				}
				btn.TextColor = Colors.Black;
				btn.BackgroundColor = Colors.LightSkyBlue;
				activeTab = btn;

				contentArea.Children.Clear();
				contentArea.Children.Add(capturedBuilder());
			};
			tabs.Children.Add(btn);
		}

		var firstBtn = (Button)tabs.Children[0];
		firstBtn.TextColor = Colors.Black;
		firstBtn.BackgroundColor = Colors.LightSkyBlue;
		activeTab = firstBtn;
		contentArea.Children.Add(pages[0].builder());

		Content = new VerticalStackLayout
		{
			Spacing = 8,
			Padding = new Thickness(24),
			Children =
			{
				new Label { Text = "CollectionView", FontSize = 24, FontAttributes = FontAttributes.Bold },
				new BoxView { HeightRequest = 2, Color = Colors.DodgerBlue },
				tabs,
				new BoxView { HeightRequest = 1, Color = Colors.LightGray },
				contentArea,
			}
		};
	}

	View BuildSimpleList()
	{
		var items = Enumerable.Range(1, 50)
			.Select(i => $"Item {i} — {GetDescription(i)}")
			.ToList();

		var selectedLabel = new Label { Text = "Tap an item to select it", FontSize = 14, TextColor = Colors.Gray };
		var countLabel = new Label { Text = $"Showing {items.Count} items", FontSize = 12, TextColor = Colors.DodgerBlue };
		var searchBar = new SearchBar { Placeholder = "Filter items..." };
		var stackList = new VerticalStackLayout { Spacing = 0 };
		PopulateSimpleList(stackList, items, selectedLabel);

		searchBar.TextChanged += (s, e) =>
		{
			var filtered = string.IsNullOrWhiteSpace(e.NewTextValue)
				? items
				: items.Where(i => i.Contains(e.NewTextValue, StringComparison.OrdinalIgnoreCase)).ToList();
			countLabel.Text = $"Showing {filtered.Count} items";
			PopulateSimpleList(stackList, filtered, selectedLabel);
		};

		return new VerticalStackLayout
		{
			Spacing = 8,
			Children = { searchBar, countLabel, selectedLabel, new ScrollView { HeightRequest = 350, Content = stackList } }
		};
	}

	void PopulateSimpleList(VerticalStackLayout stack, IList<string> items, Label selectedLabel)
	{
		stack.Children.Clear();
		foreach (var item in items)
		{
			var btn = new Button
			{
				Text = item,
				BackgroundColor = Colors.Transparent,
				TextColor = Colors.Black,
				FontSize = 14,
				HorizontalOptions = LayoutOptions.Fill,
			};
			var captured = item;
			btn.Clicked += (s, e) =>
			{
				selectedLabel.Text = $"Selected: {captured}";
				selectedLabel.TextColor = Colors.DodgerBlue;
			};
			stack.Children.Add(btn);
			stack.Children.Add(new BoxView { HeightRequest = 1, Color = Color.FromArgb("#f0f0f0") });
		}
	}

	record ContactItem(string Name, string Email, string Initials, Color AvatarColor);

	View BuildTemplatedList()
	{
		var contacts = new List<ContactItem>
		{
			new("Alice Johnson", "alice@example.com", "AJ", Colors.Coral),
			new("Bob Smith", "bob@example.com", "BS", Colors.CornflowerBlue),
			new("Carol White", "carol@example.com", "CW", Colors.MediumSeaGreen),
			new("David Brown", "david@example.com", "DB", Colors.MediumOrchid),
			new("Eve Davis", "eve@example.com", "ED", Colors.SandyBrown),
			new("Frank Miller", "frank@example.com", "FM", Colors.SlateBlue),
			new("Grace Lee", "grace@example.com", "GL", Colors.Teal),
			new("Hank Wilson", "hank@example.com", "HW", Colors.IndianRed),
			new("Ivy Chen", "ivy@example.com", "IC", Colors.DarkCyan),
			new("Jack Taylor", "jack@example.com", "JT", Colors.OliveDrab),
			new("Karen Moore", "karen@example.com", "KM", Colors.Crimson),
			new("Leo Martinez", "leo@example.com", "LM", Colors.DodgerBlue),
		};

		var selectedLabel = new Label { Text = "Tap a contact to view details", FontSize = 14, TextColor = Colors.Gray };
		var stack = new VerticalStackLayout { Spacing = 4 };

		foreach (var contact in contacts)
		{
			var card = BuildContactCard(contact, selectedLabel);
			stack.Children.Add(card);
		}

		return new VerticalStackLayout
		{
			Spacing = 8,
			Children =
			{
				new Label { Text = "Contacts", FontSize = 16, FontAttributes = FontAttributes.Bold },
				selectedLabel,
				new ScrollView { HeightRequest = 380, Content = stack }
			}
		};
	}

	View BuildContactCard(ContactItem contact, Label selectedLabel)
	{
		var avatar = new Border
		{
			StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 20 },
			BackgroundColor = contact.AvatarColor,
			WidthRequest = 40,
			HeightRequest = 40,
			StrokeThickness = 0,
			Content = new Label
			{
				Text = contact.Initials,
				TextColor = Colors.White,
				FontSize = 16,
				FontAttributes = FontAttributes.Bold,
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center,
			}
		};

		var info = new VerticalStackLayout
		{
			Spacing = 2,
			Children =
			{
				new Label { Text = contact.Name, FontSize = 15, FontAttributes = FontAttributes.Bold },
				new Label { Text = contact.Email, FontSize = 12, TextColor = Colors.Gray },
			}
		};

		var row = new HorizontalStackLayout
		{
			Spacing = 12,
			Padding = new Thickness(12, 8),
			Children = { avatar, info }
		};

		var tapGesture = new TapGestureRecognizer();
		tapGesture.Tapped += (s, e) =>
		{
			selectedLabel.Text = $"📧 {contact.Name} ({contact.Email})";
			selectedLabel.TextColor = contact.AvatarColor;
		};

		var container = new Border
		{
			StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 8 },
			Stroke = Color.FromArgb("#e0e0e0"),
			StrokeThickness = 1,
			BackgroundColor = Colors.White,
			Content = row,
		};
		container.GestureRecognizers.Add(tapGesture);
		return container;
	}

	View BuildMultiSelectList()
	{
		var tasks = new List<(string name, string priority)>
		{
			("Review pull request #42", "High"),
			("Update documentation", "Medium"),
			("Fix login page CSS", "High"),
			("Write unit tests for API", "Medium"),
			("Deploy staging build", "Low"),
			("Refactor database queries", "High"),
			("Design new onboarding flow", "Medium"),
			("Update dependencies", "Low"),
			("Fix memory leak in worker", "High"),
			("Add dark mode support", "Medium"),
			("Create CI/CD pipeline", "Medium"),
			("Optimize image loading", "Low"),
			("Implement search feature", "High"),
			("Localize UI strings", "Low"),
			("Add telemetry events", "Medium"),
		};

		var selectionLabel = new Label { Text = "0 tasks selected", FontSize = 14, TextColor = Colors.Gray };
		var selected = new HashSet<int>();

		var selectAllBox = new CheckBox { IsChecked = false };
		var selectAllRow = new HorizontalStackLayout
		{
			Spacing = 8,
			Padding = new Thickness(12, 4),
			Children = { selectAllBox, new Label { Text = "Select All", FontSize = 14, VerticalTextAlignment = TextAlignment.Center } }
		};

		var stack = new VerticalStackLayout { Spacing = 0 };
		var checkBoxes = new List<CheckBox>();

		for (int i = 0; i < tasks.Count; i++)
		{
			var (name, priority) = tasks[i];
			var idx = i;

			var cb = new CheckBox();
			checkBoxes.Add(cb);

			var priorityColor = priority switch
			{
				"High" => Colors.Red,
				"Medium" => Colors.Orange,
				_ => Colors.Gray,
			};

			var row = new HorizontalStackLayout
			{
				Spacing = 8,
				Padding = new Thickness(12, 6),
				Children =
				{
					cb,
					new Border
					{
						StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 4 },
						BackgroundColor = priorityColor,
						Padding = new Thickness(6, 2),
						StrokeThickness = 0,
						Content = new Label { Text = priority, FontSize = 10, TextColor = Colors.White },
					},
					new Label { Text = name, FontSize = 14, VerticalTextAlignment = TextAlignment.Center },
				}
			};

			cb.CheckedChanged += (s, e) =>
			{
				if (e.Value) selected.Add(idx); else selected.Remove(idx);
				selectionLabel.Text = selected.Count == 0
					? "0 tasks selected"
					: $"{selected.Count} task{(selected.Count == 1 ? "" : "s")} selected";
				selectionLabel.TextColor = selected.Count > 0 ? Colors.DodgerBlue : Colors.Gray;
			};

			stack.Children.Add(row);
			stack.Children.Add(new BoxView { HeightRequest = 1, Color = Color.FromArgb("#f0f0f0") });
		}

		selectAllBox.CheckedChanged += (s, e) =>
		{
			foreach (var cb in checkBoxes)
				cb.IsChecked = e.Value;
		};

		var deleteBtn = new Button
		{
			Text = "🗑️ Delete Selected",
			BackgroundColor = Colors.Red,
			TextColor = Colors.White,
			FontSize = 13,
			IsEnabled = true,
		};
		deleteBtn.Clicked += (s, e) =>
		{
			selectionLabel.Text = selected.Count > 0
				? $"Would delete {selected.Count} task(s)"
				: "No tasks selected";
		};

		return new VerticalStackLayout
		{
			Spacing = 8,
			Children =
			{
				new Label { Text = "Task List", FontSize = 16, FontAttributes = FontAttributes.Bold },
				selectionLabel,
				selectAllRow,
				new BoxView { HeightRequest = 1, Color = Colors.LightGray },
				new ScrollView { HeightRequest = 320, Content = stack },
				deleteBtn,
			}
		};
	}

	static string GetDescription(int i) => (i % 5) switch
	{
		0 => "🔴 Important task",
		1 => "🟢 Completed item",
		2 => "🔵 In progress",
		3 => "🟡 Pending review",
		_ => "⚪ Backlog item",
	};
}
