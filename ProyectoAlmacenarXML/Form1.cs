using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Xml.Linq;
using System.IO;
using System.Linq;

namespace GuardadordeXML
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void buttonSelectFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML Files (*.xml)|*.xml";
            openFileDialog.Title = "Seleccione un archivo XML";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                textBoxFilePath.Text = openFileDialog.FileName;
            }
        }

        private void buttonProcessFile_Click(object sender, EventArgs e)
        {
            string xmlPath = textBoxFilePath.Text;

            if (string.IsNullOrEmpty(xmlPath) || !File.Exists(xmlPath))
            {
                MessageBox.Show("Seleccione un archivo XML válido.");
                return;
            }

            // Leer el XML y obtener los datos
            XDocument xmlDoc = XDocument.Load(xmlPath);
            XElement comprobanteElement = xmlDoc.Element(XName.Get("Comprobante", "http://www.sat.gob.mx/cfd/4"));
            XElement timbreElement = comprobanteElement.Descendants(XName.Get("TimbreFiscalDigital", "http://www.sat.gob.mx/TimbreFiscalDigital")).FirstOrDefault();
            string uuid = timbreElement.Attribute("UUID").Value;

            // Comprobar si la factura ya existe en la base de datos
            if (FacturaExiste(uuid))
            {
                MessageBox.Show("La factura ya ha sido guardada previamente.");
            }
            else
            {
                GuardarFacturaEnBaseDeDatos(comprobanteElement, timbreElement);
                MessageBox.Show("Factura guardada exitosamente.");

                // Mover el archivo a la carpeta XML_Procesados con un nuevo nombre
                string processedDir = @"C:\Users\diego\Downloads\Procesados2";
                if (!Directory.Exists(processedDir))
                {
                    Directory.CreateDirectory(processedDir);
                }

                // Nuevo nombre del archivo con el identificador único
                string newFileName = $"Factura_Procesada-{uuid}.xml";
                string destFile = Path.Combine(processedDir, newFileName);
                File.Move(xmlPath, destFile);
            }
        }

        private bool FacturaExiste(string uuid)
        {
            string connectionString = "Server=(local); Database=XMLDataBase; Integrated Security=true;Encrypt=False";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Factura WHERE UUID = @UUID", conn);
                cmd.Parameters.AddWithValue("@UUID", uuid);

                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        private void GuardarFacturaEnBaseDeDatos(XElement comprobanteElement, XElement timbreElement)
        {
            string connectionString = "Server=(local); Database=XMLDataBase; Integrated Security=true;Encrypt=False";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Guardar Factura
                SqlCommand cmdFactura = new SqlCommand(
                    "INSERT INTO Factura (UUID, Version, Folio, Fecha, FormaDePago, SubTotal, Total, Descuento, MetodoDePago, Moneda, TipoDeCambio, TipoDeComprobante, LugarExpedicion, Exportacion, NoCertificado, Certificado, Sello) " +
                    "VALUES (@UUID, @Version, @Folio, @Fecha, @FormaDePago, @SubTotal, @Total, @Descuento, @MetodoDePago, @Moneda, @TipoDeCambio, @TipoDeComprobante, @LugarExpedicion, @Exportacion, @NoCertificado, @Certificado, @Sello)", conn);
                cmdFactura.Parameters.AddWithValue("@UUID", timbreElement.Attribute("UUID").Value);
                cmdFactura.Parameters.AddWithValue("@Version", comprobanteElement.Attribute("Version").Value);
                cmdFactura.Parameters.AddWithValue("@Folio", comprobanteElement.Attribute("Folio")?.Value ?? string.Empty);
                cmdFactura.Parameters.AddWithValue("@Fecha", DateTime.Parse(comprobanteElement.Attribute("Fecha").Value));
                cmdFactura.Parameters.AddWithValue("@FormaDePago", comprobanteElement.Attribute("FormaPago").Value);
                cmdFactura.Parameters.AddWithValue("@SubTotal", decimal.Parse(comprobanteElement.Attribute("SubTotal").Value));
                cmdFactura.Parameters.AddWithValue("@Total", decimal.Parse(comprobanteElement.Attribute("Total").Value));
                cmdFactura.Parameters.AddWithValue("@Descuento", decimal.Parse(comprobanteElement.Attribute("Descuento")?.Value ?? "0"));
                cmdFactura.Parameters.AddWithValue("@MetodoDePago", comprobanteElement.Attribute("MetodoPago").Value);
                cmdFactura.Parameters.AddWithValue("@Moneda", comprobanteElement.Attribute("Moneda").Value);
                cmdFactura.Parameters.AddWithValue("@TipoDeCambio", decimal.Parse(comprobanteElement.Attribute("TipoCambio")?.Value ?? "0"));
                cmdFactura.Parameters.AddWithValue("@TipoDeComprobante", comprobanteElement.Attribute("TipoDeComprobante").Value);
                cmdFactura.Parameters.AddWithValue("@LugarExpedicion", comprobanteElement.Attribute("LugarExpedicion").Value);
                cmdFactura.Parameters.AddWithValue("@Exportacion", comprobanteElement.Attribute("Exportacion")?.Value ?? string.Empty);
                cmdFactura.Parameters.AddWithValue("@NoCertificado", comprobanteElement.Attribute("NoCertificado").Value);
                cmdFactura.Parameters.AddWithValue("@Certificado", comprobanteElement.Attribute("Certificado").Value);
                cmdFactura.Parameters.AddWithValue("@Sello", comprobanteElement.Attribute("Sello").Value);
                cmdFactura.ExecuteNonQuery();

                // Guardar Emisor
                XElement emisorElement = comprobanteElement.Element(XName.Get("Emisor", "http://www.sat.gob.mx/cfd/4"));
                SqlCommand cmdEmisor = new SqlCommand(
                    "INSERT INTO Emisor (UUID, Nombre, RFC, RegimenFiscal) " +
                    "VALUES (@UUID, @Nombre, @RFC, @RegimenFiscal)", conn);
                cmdEmisor.Parameters.AddWithValue("@UUID", timbreElement.Attribute("UUID").Value);
                cmdEmisor.Parameters.AddWithValue("@Nombre", emisorElement.Attribute("Nombre").Value);
                cmdEmisor.Parameters.AddWithValue("@RFC", emisorElement.Attribute("Rfc").Value);
                cmdEmisor.Parameters.AddWithValue("@RegimenFiscal", emisorElement.Attribute("RegimenFiscal").Value);
                cmdEmisor.ExecuteNonQuery();

                // Guardar Receptor
                XElement receptorElement = comprobanteElement.Element(XName.Get("Receptor", "http://www.sat.gob.mx/cfd/4"));
                SqlCommand cmdReceptor = new SqlCommand(
                    "INSERT INTO Receptor (UUID, Nombre, RFC, UsoCFDI, DomicilioFiscal, RegimenFiscal) " +
                    "VALUES (@UUID, @Nombre, @RFC, @UsoCFDI, @DomicilioFiscal, @RegimenFiscal)", conn);
                cmdReceptor.Parameters.AddWithValue("@UUID", timbreElement.Attribute("UUID").Value);
                cmdReceptor.Parameters.AddWithValue("@Nombre", receptorElement.Attribute("Nombre").Value);
                cmdReceptor.Parameters.AddWithValue("@RFC", receptorElement.Attribute("Rfc").Value);
                cmdReceptor.Parameters.AddWithValue("@UsoCFDI", receptorElement.Attribute("UsoCFDI").Value);
                cmdReceptor.Parameters.AddWithValue("@DomicilioFiscal", receptorElement.Attribute("DomicilioFiscalReceptor").Value); // Corregido aquí
                cmdReceptor.Parameters.AddWithValue("@RegimenFiscal", receptorElement.Attribute("RegimenFiscalReceptor").Value); // Corregido aquí
                cmdReceptor.ExecuteNonQuery();

                // Guardar Concepto
                var conceptos = comprobanteElement.Element(XName.Get("Conceptos", "http://www.sat.gob.mx/cfd/4")).Elements(XName.Get("Concepto", "http://www.sat.gob.mx/cfd/4"));
                foreach (var concepto in conceptos)
                {
                    SqlCommand cmdConcepto = new SqlCommand(
                        "INSERT INTO Concepto (UUID, ClaveProdServ, Cantidad, ClaveUnidad, Unidad, NoIdentificacion, Descripcion, ValorUnitario, Importe, Descuento, ObjetoImp) " +
                        "VALUES (@UUID, @ClaveProdServ, @Cantidad, @ClaveUnidad, @Unidad, @NoIdentificacion, @Descripcion, @ValorUnitario, @Importe, @Descuento, @ObjetoImp); SELECT SCOPE_IDENTITY();", conn);
                    cmdConcepto.Parameters.AddWithValue("@UUID", timbreElement.Attribute("UUID").Value);
                    cmdConcepto.Parameters.AddWithValue("@ClaveProdServ", concepto.Attribute("ClaveProdServ").Value);
                    cmdConcepto.Parameters.AddWithValue("@Cantidad", decimal.Parse(concepto.Attribute("Cantidad").Value));
                    cmdConcepto.Parameters.AddWithValue("@ClaveUnidad", concepto.Attribute("ClaveUnidad").Value);
                    cmdConcepto.Parameters.AddWithValue("@Unidad", concepto.Attribute("Unidad").Value);
                    cmdConcepto.Parameters.AddWithValue("@NoIdentificacion", concepto.Attribute("NoIdentificacion")?.Value ?? string.Empty);
                    cmdConcepto.Parameters.AddWithValue("@Descripcion", concepto.Attribute("Descripcion").Value);
                    cmdConcepto.Parameters.AddWithValue("@ValorUnitario", decimal.Parse(concepto.Attribute("ValorUnitario").Value));
                    cmdConcepto.Parameters.AddWithValue("@Importe", decimal.Parse(concepto.Attribute("Importe").Value));
                    cmdConcepto.Parameters.AddWithValue("@Descuento", decimal.Parse(concepto.Attribute("Descuento")?.Value ?? "0"));
                    cmdConcepto.Parameters.AddWithValue("@ObjetoImp", concepto.Attribute("ObjetoImp").Value);
                    int conceptoID = Convert.ToInt32(cmdConcepto.ExecuteScalar());

                    // Guardar Impuesto
                    var impuestosConcepto = concepto.Element(XName.Get("Impuestos", "http://www.sat.gob.mx/cfd/4"));
                    if (impuestosConcepto != null)
                    {
                        var impuestos = impuestosConcepto.Elements(XName.Get("Traslados", "http://www.sat.gob.mx/cfd/4")).Elements(XName.Get("Traslado", "http://www.sat.gob.mx/cfd/4"));
                        foreach (var impuesto in impuestos)
                        {
                            SqlCommand cmdImpuesto = new SqlCommand(
                                "INSERT INTO Impuesto (ConceptoID, Impuesto, TipoFactor, TasaOCuota, Importe, Base) " +
                                "VALUES (@ConceptoID, @Impuesto, @TipoFactor, @TasaOCuota, @Importe, @Base)", conn);
                            cmdImpuesto.Parameters.AddWithValue("@ConceptoID", conceptoID);
                            cmdImpuesto.Parameters.AddWithValue("@Impuesto", impuesto.Attribute("Impuesto").Value);
                            cmdImpuesto.Parameters.AddWithValue("@TipoFactor", impuesto.Attribute("TipoFactor").Value);
                            cmdImpuesto.Parameters.AddWithValue("@TasaOCuota", decimal.Parse(impuesto.Attribute("TasaOCuota").Value));
                            cmdImpuesto.Parameters.AddWithValue("@Importe", decimal.Parse(impuesto.Attribute("Importe").Value));
                            cmdImpuesto.Parameters.AddWithValue("@Base", decimal.Parse(impuesto.Attribute("Base").Value));
                            cmdImpuesto.ExecuteNonQuery();
                        }
                    }
                }

                // Guardar Timbre Fiscal Digital
                SqlCommand cmdTimbre = new SqlCommand(
                    "INSERT INTO TimbreFiscalDigital (UUID, Version, FechaTimbrado, SelloCFD, NoCertificadoSAT, SelloSAT, RFCProveedorCertif) " +
                    "VALUES (@UUID, @Version, @FechaTimbrado, @SelloCFD, @NoCertificadoSAT, @SelloSAT, @RFCProveedorCertif)", conn);
                cmdTimbre.Parameters.AddWithValue("@UUID", timbreElement.Attribute("UUID").Value);
                cmdTimbre.Parameters.AddWithValue("@Version", timbreElement.Attribute("Version").Value);
                cmdTimbre.Parameters.AddWithValue("@FechaTimbrado", DateTime.Parse(timbreElement.Attribute("FechaTimbrado").Value));
                cmdTimbre.Parameters.AddWithValue("@SelloCFD", timbreElement.Attribute("SelloCFD").Value);
                cmdTimbre.Parameters.AddWithValue("@NoCertificadoSAT", timbreElement.Attribute("NoCertificadoSAT").Value);
                cmdTimbre.Parameters.AddWithValue("@SelloSAT", timbreElement.Attribute("SelloSAT").Value);
                cmdTimbre.Parameters.AddWithValue("@RFCProveedorCertif", timbreElement.Attribute("RfcProvCertif").Value);
                cmdTimbre.ExecuteNonQuery();

            }
        }
}
}