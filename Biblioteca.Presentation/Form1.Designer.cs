
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Biblioteca.Presentation
{
    partial class Form1
    {
        private IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private Label label7;

        private TextBox txtTitulo;
        private TextBox txtISBN;
        private TextBox txtAnio;
        private TextBox txtTotal;
        private TextBox txtEditorialID;
        private TextBox txtDisponibles;

        private Button btnGuardar;
        private Button btnEliminar;

        private DataGridView dgvLibros;

        private void InitializeComponent()
        {
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            label6 = new Label();
            label7 = new Label();
            txtTitulo = new TextBox();
            txtISBN = new TextBox();
            txtAnio = new TextBox();
            txtTotal = new TextBox();
            txtEditorialID = new TextBox();
            txtDisponibles = new TextBox();
            btnGuardar = new Button();
            btnEliminar = new Button();
            dgvLibros = new DataGridView();
            btnEditar = new Button();
            ((ISupportInitialize)dgvLibros).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(589, 9);
            label1.Name = "label1";
            label1.Size = new Size(137, 20);
            label1.TabIndex = 15;
            label1.Text = "GESTOR DE LIBROS";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(46, 84);
            label2.Name = "label2";
            label2.Size = new Size(59, 20);
            label2.TabIndex = 14;
            label2.Text = "TITULO:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(46, 137);
            label3.Name = "label3";
            label3.Size = new Size(44, 20);
            label3.TabIndex = 12;
            label3.Text = "ISBN:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(672, 81);
            label4.Name = "label4";
            label4.Size = new Size(44, 20);
            label4.TabIndex = 10;
            label4.Text = "AÑO:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(575, 137);
            label5.Name = "label5";
            label5.Size = new Size(141, 20);
            label5.TabIndex = 8;
            label5.Text = "TOTAL EJEMPLARES:";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(46, 192);
            label6.Name = "label6";
            label6.Size = new Size(102, 20);
            label6.TabIndex = 6;
            label6.Text = "ID EDITORIAL:";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(615, 192);
            label7.Name = "label7";
            label7.Size = new Size(101, 20);
            label7.TabIndex = 4;
            label7.Text = "DISPONIBLES:";
            // 
            // txtTitulo
            // 
            txtTitulo.Location = new Point(154, 81);
            txtTitulo.Name = "txtTitulo";
            txtTitulo.Size = new Size(317, 27);
            txtTitulo.TabIndex = 13;
            // 
            // txtISBN
            // 
            txtISBN.Location = new Point(154, 137);
            txtISBN.Name = "txtISBN";
            txtISBN.Size = new Size(317, 27);
            txtISBN.TabIndex = 11;
            // 
            // txtAnio
            // 
            txtAnio.Location = new Point(722, 77);
            txtAnio.Name = "txtAnio";
            txtAnio.Size = new Size(270, 27);
            txtAnio.TabIndex = 9;
            // 
            // txtTotal
            // 
            txtTotal.Location = new Point(722, 130);
            txtTotal.Name = "txtTotal";
            txtTotal.Size = new Size(270, 27);
            txtTotal.TabIndex = 7;
            // 
            // txtEditorialID
            // 
            txtEditorialID.Location = new Point(154, 185);
            txtEditorialID.Name = "txtEditorialID";
            txtEditorialID.Size = new Size(317, 27);
            txtEditorialID.TabIndex = 5;
            // 
            // txtDisponibles
            // 
            txtDisponibles.Location = new Point(722, 189);
            txtDisponibles.Name = "txtDisponibles";
            txtDisponibles.Size = new Size(270, 27);
            txtDisponibles.TabIndex = 3;
            // 
            // btnGuardar
            // 
            btnGuardar.Location = new Point(1128, 40);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(161, 39);
            btnGuardar.TabIndex = 2;
            btnGuardar.Text = "GUARDAR";
            btnGuardar.UseVisualStyleBackColor = true;
            // 
            // btnEliminar
            // 
            btnEliminar.Location = new Point(1128, 185);
            btnEliminar.Name = "btnEliminar";
            btnEliminar.Size = new Size(161, 48);
            btnEliminar.TabIndex = 1;
            btnEliminar.Text = "ELIMINAR";
            btnEliminar.UseVisualStyleBackColor = true;
            // 
            // dgvLibros
            // 
            dgvLibros.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvLibros.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvLibros.Location = new Point(130, 263);
            dgvLibros.Name = "dgvLibros";
            dgvLibros.RowHeadersWidth = 51;
            dgvLibros.Size = new Size(1051, 415);
            dgvLibros.TabIndex = 0;
            // 
            // btnEditar
            // 
            btnEditar.Location = new Point(1128, 112);
            btnEditar.Name = "btnEditar";
            btnEditar.Size = new Size(161, 45);
            btnEditar.TabIndex = 16;
            btnEditar.Text = "EDITAR";
            btnEditar.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1329, 709);
            Controls.Add(btnEditar);
            Controls.Add(dgvLibros);
            Controls.Add(btnEliminar);
            Controls.Add(btnGuardar);
            Controls.Add(txtDisponibles);
            Controls.Add(label7);
            Controls.Add(txtEditorialID);
            Controls.Add(label6);
            Controls.Add(txtTotal);
            Controls.Add(label5);
            Controls.Add(txtAnio);
            Controls.Add(label4);
            Controls.Add(txtISBN);
            Controls.Add(label3);
            Controls.Add(txtTitulo);
            Controls.Add(label2);
            Controls.Add(label1);
            Name = "Form1";
            Text = "Gestor de Libros";
            Load += Form1_Load;
            ((ISupportInitialize)dgvLibros).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private Button btnEditar;
    }
}
