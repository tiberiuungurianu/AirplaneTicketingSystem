using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Windows.Forms;
using System.Xml.Serialization;
using static System.Net.Mime.MediaTypeNames;

// Models
[Serializable]
public class Seat
{
    public string SeatNumber { get; set; }
    public bool IsOccupied { get; set; }
    public string PassengerName { get; set; }
    public bool IsFirstClass { get; set; }
    public int Row { get; set; }
    public char Column { get; set; }

    public Seat() { } // Required for serialization

    public Seat(int row, char column, bool isFirstClass)
    {
        Row = row;
        Column = column;
        IsFirstClass = isFirstClass;
        SeatNumber = $"{row}{column}";
        IsOccupied = false;
        PassengerName = "";
    }
}

[Serializable]
public class Airplane
{
    public List<Seat> Seats { get; set; }
    private const int FIRST_CLASS_ROWS = 5;
    private const int ECONOMY_ROWS = 30;

    public Airplane()
    {
        Seats = new List<Seat>();
        InitializeSeats();
    }

    private void InitializeSeats()
    {
        // First Class (5 rows x 4 seats)
        for (int row = 1; row <= FIRST_CLASS_ROWS; row++)
        {
            foreach (char col in new[] { 'A', 'B', 'E', 'F' }) // C & D are aisle
            {
                Seats.Add(new Seat(row, col, true));
            }
        }

        // Economy Class (30 rows x 6 seats)
        for (int row = 6; row <= ECONOMY_ROWS + 5; row++)
        {
            foreach (char col in new[] { 'A', 'B', 'C', 'E', 'F', 'G' }) // D is aisle
            {
                Seats.Add(new Seat(row, col, false));
            }
        }
    }
}

// Main Form
public class MainForm : Form
{
    private Airplane airplane;
    private DataGridView gridSeats;
    private ComboBox comboSortOrder;
    private Button btnAssign;
    private Button btnSave;
    private Button btnLoad;
    private const string SAVE_FILE = "airplane_state.xml";

    public MainForm()
    {
        airplane = new Airplane();
        InitializeComponents();
        LoadSeatGrid();
    }

    private void InitializeComponents()
    {
        this.Size = new Size(800, 600);
        this.Text = "Airplane Seating Management";

        // Sort order combo box
        comboSortOrder = new ComboBox
        {
            Location = new Point(10, 10),
            Size = new Size(150, 25)
        };
        comboSortOrder.Items.AddRange(new string[] { "Seat Number", "Passenger Name" });
        comboSortOrder.SelectedIndex = 0;
        comboSortOrder.SelectedIndexChanged += ComboSortOrder_SelectedIndexChanged;

        // Buttons
        btnAssign = new Button
        {
            Text = "Assign Seats",
            Location = new Point(170, 10),
            Size = new Size(100, 25)
        };
        btnAssign.Click += BtnAssign_Click;

        btnSave = new Button
        {
            Text = "Save State",
            Location = new Point(280, 10),
            Size = new Size(100, 25)
        };
        btnSave.Click += BtnSave_Click;

        btnLoad = new Button
        {
            Text = "Load State",
            Location = new Point(390, 10),
            Size = new Size(100, 25)
        };
        btnLoad.Click += BtnLoad_Click;

        // Grid
        gridSeats = new DataGridView
        {
            Location = new Point(10, 45),
            Size = new Size(760, 500),
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true
        };

        this.Controls.AddRange(new Control[] { comboSortOrder, btnAssign, btnSave, btnLoad, gridSeats });
    }

    private void LoadSeatGrid()
    {
        var seats = airplane.Seats;
        if (comboSortOrder.SelectedItem.ToString() == "Passenger Name")
        {
            seats = seats.OrderBy(s => s.PassengerName).ToList();
        }
        else
        {
            seats = seats.OrderBy(s => s.Row).ThenBy(s => s.Column).ToList();
        }

        gridSeats.DataSource = seats.Select(s => new
        {
            s.SeatNumber,
            Class = s.IsFirstClass ? "First" : "Economy",
            Status = s.IsOccupied ? "Occupied" : "Available",
            s.PassengerName
        }).ToList();
    }

    private void ComboSortOrder_SelectedIndexChanged(object sender, EventArgs e)
    {
        LoadSeatGrid();
    }

    private void BtnAssign_Click(object sender, EventArgs e)
    {
        using (var assignForm = new AssignSeatsForm(airplane))
        {
            if (assignForm.ShowDialog() == DialogResult.OK)
            {
                LoadSeatGrid();
            }
        }
    }

    private void SaveState()
    {
        var serializer = new XmlSerializer(typeof(Airplane));
        using (var writer = new StreamWriter(SAVE_FILE))
        {
            serializer.Serialize(writer, airplane);
        }
        MessageBox.Show("State saved successfully!");
    }

    private void LoadState()
    {
        if (File.Exists(SAVE_FILE))
        {
            var serializer = new XmlSerializer(typeof(Airplane));
            using (var reader = new StreamReader(SAVE_FILE))
            {
                airplane = (Airplane)serializer.Deserialize(reader);
            }
            LoadSeatGrid();
            MessageBox.Show("State loaded successfully!");
        }
        else
        {
            MessageBox.Show("No saved state found.");
        }
    }

    private void BtnSave_Click(object sender, EventArgs e)
    {
        SaveState();
    }

    private void BtnLoad_Click(object sender, EventArgs e)
    {
        LoadState();
    }
}

// Seat Assignment Form
public class AssignSeatsForm : Form
{
    private readonly Airplane airplane;
    private ComboBox comboClass;
    private NumericUpDown numPassengers;
    private TextBox txtPassengerNames;
    private Button btnAssign;
    private Label lblAvailableSeats;

    public AssignSeatsForm(Airplane airplane)
    {
        this.airplane = airplane;
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        this.Size = new Size(400, 300);
        this.Text = "Assign Seats";
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        var lblClass = new Label
        {
            Text = "Class:",
            Location = new Point(10, 10),
            Size = new Size(100, 20)
        };

        comboClass = new ComboBox
        {
            Location = new Point(120, 10),
            Size = new Size(150, 20)
        };
        comboClass.Items.AddRange(new string[] { "First Class", "Economy Class" });
        comboClass.SelectedIndex = 0;
        comboClass.SelectedIndexChanged += ComboClass_SelectedIndexChanged;

        var lblPassengers = new Label
        {
            Text = "Number of Passengers:",
            Location = new Point(10, 40),
            Size = new Size(100, 20)
        };

        numPassengers = new NumericUpDown
        {
            Location = new Point(120, 40),
            Size = new Size(50, 20),
            Minimum = 1,
            Maximum = 2
        };

        var lblNames = new Label
        {
            Text = "Passenger Names\n(one per line):",
            Location = new Point(10, 70),
            Size = new Size(100, 40)
        };

        txtPassengerNames = new TextBox
        {
            Location = new Point(120, 70),
            Size = new Size(250, 100),
            Multiline = true
        };

        lblAvailableSeats = new Label
        {
            Location = new Point(10, 180),
            Size = new Size(360, 20)
        };

        btnAssign = new Button
        {
            Text = "Assign Seats",
            Location = new Point(120, 210),
            Size = new Size(150, 30)
        };
        btnAssign.Click += BtnAssign_Click;

        this.Controls.AddRange(new Control[] {
            lblClass, comboClass,
            lblPassengers, numPassengers,
            lblNames, txtPassengerNames,
            lblAvailableSeats,
            btnAssign
        });

        UpdateAvailableSeats();
    }

    private void ComboClass_SelectedIndexChanged(object sender, EventArgs e)
    {
        bool isFirstClass = comboClass.SelectedIndex == 0;
        numPassengers.Maximum = isFirstClass ? 2 : 3;
        UpdateAvailableSeats();
    }

    private void UpdateAvailableSeats()
    {
        bool isFirstClass = comboClass.SelectedIndex == 0;
        int availableSeats = airplane.Seats.Count(s =>
            s.IsFirstClass == isFirstClass && !s.IsOccupied);
        lblAvailableSeats.Text = $"Available seats in {(isFirstClass ? "First" : "Economy")} Class: {availableSeats}";
    }

    private void BtnAssign_Click(object sender, EventArgs e)
    {
        bool isFirstClass = comboClass.SelectedIndex == 0;
        int requestedSeats = (int)numPassengers.Value;
        string[] passengerNames = txtPassengerNames.Text.Split(new[] { Environment.NewLine },
            StringSplitOptions.RemoveEmptyEntries);

        if (passengerNames.Length != requestedSeats)
        {
            MessageBox.Show($"Please enter exactly {requestedSeats} passenger name(s).");
            return;
        }

        var availableSeats = airplane.Seats
            .Where(s => s.IsFirstClass == isFirstClass && !s.IsOccupied)
            .Take(requestedSeats)
            .ToList();

        if (availableSeats.Count < requestedSeats)
        {
            MessageBox.Show($"Not enough seats available in {(isFirstClass ? "First" : "Economy")} Class.");
            return;
        }

        for (int i = 0; i < requestedSeats; i++)
        {
            availableSeats[i].IsOccupied = true;
            availableSeats[i].PassengerName = passengerNames[i];
        }

        MessageBox.Show($"Seats assigned successfully!\n" +
            $"Assigned seats: {string.Join(", ", availableSeats.Select(s => s.SeatNumber))}");

        this.DialogResult = DialogResult.OK;
        this.Close();
    }
}

// Program Entry Point
static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}
