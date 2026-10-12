using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace ParcialExamen
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new VentanaProductos());
        }
    }

    public class VentanaProductos : Form
    {
        private readonly string connectionString =
            @"Server=(localdb)\ProjectModels;Database=ParcialProgramacion1;Trusted_Connection=True;";

        private TextBox txtId, txtNombre, txtCategoria, txtPrecio, txtExistencias;
        private Button btnGuardar, btnConsultar, btnActualizar, btnEliminar, btnLimpiar;
        private DataGridView dgvProductos;

        public VentanaProductos()
        {
            CrearInterfaz();
        }

        // ---------- CREA EL FORMULARIO (cajas, botones y tabla) ----------
        private void CrearInterfaz()
        {
            this.Text = "Control de Productos";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ClientSize = new Size(660, 540);

            txtId = CrearCampo("Id:", 20, true);
            txtNombre = CrearCampo("Nombre:", 55, false);
            txtCategoria = CrearCampo("Categoría:", 90, false);
            txtPrecio = CrearCampo("Precio:", 125, false);
            txtExistencias = CrearCampo("Existencias:", 160, false);

            btnGuardar = CrearBoton("Guardar", 20);
            btnConsultar = CrearBoton("Consultar", 140);
            btnActualizar = CrearBoton("Actualizar", 260);
            btnEliminar = CrearBoton("Eliminar", 380);
            btnLimpiar = CrearBoton("Limpiar", 500);

            dgvProductos = new DataGridView();
            dgvProductos.Location = new Point(20, 260);
            dgvProductos.Size = new Size(620, 260);
            dgvProductos.ReadOnly = true;
            dgvProductos.AllowUserToAddRows = false;
            dgvProductos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvProductos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.Controls.Add(dgvProductos);

            // Conecta los eventos
            this.Load += VentanaProductos_Load;
            btnGuardar.Click += btnGuardar_Click;
            btnConsultar.Click += btnConsultar_Click;
            btnActualizar.Click += btnActualizar_Click;
            btnEliminar.Click += btnEliminar_Click;
            btnLimpiar.Click += btnLimpiar_Click;
            dgvProductos.CellClick += dgvProductos_CellClick;
        }

        private TextBox CrearCampo(string etiqueta, int y, bool soloLectura)
        {
            Label lbl = new Label();
            lbl.Text = etiqueta;
            lbl.Location = new Point(20, y + 3);
            lbl.AutoSize = true;

            TextBox txt = new TextBox();
            txt.Location = new Point(130, y);
            txt.Width = 260;
            txt.ReadOnly = soloLectura;

            this.Controls.Add(lbl);
            this.Controls.Add(txt);
            return txt;
        }

        private Button CrearBoton(string texto, int x)
        {
            Button btn = new Button();
            btn.Text = texto;
            btn.Location = new Point(x, 205);
            btn.Size = new Size(110, 35);
            this.Controls.Add(btn);
            return btn;
        }

        private void VentanaProductos_Load(object sender, EventArgs e)
        {
            CargarProductos();
        }

        // ---------- VALIDACIONES ----------
        private bool ValidarCampos(out decimal precio, out int existencias)
        {
            precio = 0;
            existencias = 0;

            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre es obligatorio.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNombre.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtCategoria.Text))
            {
                MessageBox.Show("La categoría es obligatoria.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCategoria.Focus();
                return false;
            }

            if (!decimal.TryParse(txtPrecio.Text, NumberStyles.Number,
                    CultureInfo.CurrentCulture, out precio) || precio <= 0)
            {
                MessageBox.Show("El precio debe ser numérico y mayor que cero.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPrecio.Focus();
                return false;
            }

            if (!int.TryParse(txtExistencias.Text, out existencias) || existencias < 0)
            {
                MessageBox.Show("Las existencias deben ser un número entero igual o mayor que cero.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtExistencias.Focus();
                return false;
            }

            return true;
        }

        // ---------- CONSULTAR ----------
        private void CargarProductos()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string sql = "SELECT IdProducto, Nombre, Categoria, Precio, Existencias FROM Productos";
                    SqlDataAdapter da = new SqlDataAdapter(sql, con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvProductos.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al consultar: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnConsultar_Click(object sender, EventArgs e)
        {
            CargarProductos();
        }

        // ---------- GUARDAR ----------
        private void btnGuardar_Click(object sender, EventArgs e)
        {
            decimal precio;
            int existencias;
            if (!ValidarCampos(out precio, out existencias)) return;

            string sql = @"INSERT INTO Productos (Nombre, Categoria, Precio, Existencias)
                           VALUES (@Nombre, @Categoria, @Precio, @Existencias)";
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@Nombre", txtNombre.Text.Trim());
                    cmd.Parameters.AddWithValue("@Categoria", txtCategoria.Text.Trim());
                    cmd.Parameters.AddWithValue("@Precio", precio);
                    cmd.Parameters.AddWithValue("@Existencias", existencias);

                    con.Open();
                    int filas = cmd.ExecuteNonQuery();
                    if (filas > 0)
                        MessageBox.Show("Producto guardado correctamente.", "Éxito",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                LimpiarCampos();
                CargarProductos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ---------- SELECCIONAR FILA ----------
        private void dgvProductos_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow fila = dgvProductos.Rows[e.RowIndex];
            txtId.Text = fila.Cells["IdProducto"].Value.ToString();
            txtNombre.Text = fila.Cells["Nombre"].Value.ToString();
            txtCategoria.Text = fila.Cells["Categoria"].Value.ToString();
            txtPrecio.Text = fila.Cells["Precio"].Value.ToString();
            txtExistencias.Text = fila.Cells["Existencias"].Value.ToString();
        }

        // ---------- ACTUALIZAR ----------
        private void btnActualizar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtId.Text))
            {
                MessageBox.Show("Seleccione un producto de la tabla para actualizar.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal precio;
            int existencias;
            if (!ValidarCampos(out precio, out existencias)) return;

            string sql = @"UPDATE Productos
                           SET Nombre = @Nombre, Categoria = @Categoria,
                               Precio = @Precio, Existencias = @Existencias
                           WHERE IdProducto = @Id";
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@Nombre", txtNombre.Text.Trim());
                    cmd.Parameters.AddWithValue("@Categoria", txtCategoria.Text.Trim());
                    cmd.Parameters.AddWithValue("@Precio", precio);
                    cmd.Parameters.AddWithValue("@Existencias", existencias);
                    cmd.Parameters.AddWithValue("@Id", int.Parse(txtId.Text));

                    con.Open();
                    int filas = cmd.ExecuteNonQuery();
                    if (filas > 0)
                        MessageBox.Show("Producto actualizado correctamente.", "Éxito",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    else
                        MessageBox.Show("No se encontró el producto.", "Aviso",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                LimpiarCampos();
                CargarProductos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al actualizar: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ---------- ELIMINAR (con confirmación) ----------
        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtId.Text))
            {
                MessageBox.Show("Seleccione un producto de la tabla para eliminar.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult r = MessageBox.Show("¿Está seguro de eliminar este producto?",
                "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r != DialogResult.Yes) return;

            string sql = "DELETE FROM Productos WHERE IdProducto = @Id";
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@Id", int.Parse(txtId.Text));
                    con.Open();
                    int filas = cmd.ExecuteNonQuery();
                    if (filas > 0)
                        MessageBox.Show("Producto eliminado correctamente.", "Éxito",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                LimpiarCampos();
                CargarProductos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ---------- LIMPIAR ----------
        private void LimpiarCampos()
        {
            txtId.Clear();
            txtNombre.Clear();
            txtCategoria.Clear();
            txtPrecio.Clear();
            txtExistencias.Clear();
            txtNombre.Focus();
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            LimpiarCampos();
        }
    }
}