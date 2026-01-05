Imports System.IO.Ports

Public Class FormSimuladorBascula
    Private simulador As SimuladorBascula

    ' Controles principales
    Private lblTitulo As Label
    Private lblPesoActual As Label
    Private lblPesoValor As Label
    Private lblEstado As Label
    Private lblEstadoValor As Label
    Private lblUltimoEnvio As Label
    Private lblUltimoEnvioValor As Label

    ' Configuración
    Private grpConfiguracion As GroupBox
    Private lblPuerto As Label
    Private cboPuerto As ComboBox
    Private lblIntervalo As Label
    Private numIntervalo As NumericUpDown
    Private lblPesoMin As Label
    Private numPesoMin As NumericUpDown
    Private lblPesoMax As Label
    Private numPesoMax As NumericUpDown

    ' Control de simulación
    Private grpControl As GroupBox
    Private btnIniciar As Button
    Private btnDetener As Button
    Private btnPausar As Button
    Private btnTara As Button

    ' Modos de simulación
    Private grpModos As GroupBox
    Private rbAleatorio As RadioButton
    Private rbEstable As RadioButton
    Private rbRealista As RadioButton
    Private rbIncremental As RadioButton

    ' Acciones rápidas
    Private grpAcciones As GroupBox
    Private btnSimularCarga As Button
    Private btnSimularDescarga As Button
    Private btnPesoManual As Button
    Private txtPesoManual As TextBox

    ' Log de eventos
    Private grpLog As GroupBox
    Private txtLog As TextBox
    Private btnLimpiarLog As Button

    Public Sub New()
        ' Esta llamada es requerida por el diseñador (puede estar vacía)
        InitializeComponent()

        ' Configurar formulario
        Me.Text = "Simulador de Báscula - Trailers"
        Me.Size = New Size(600, 700)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False

        ' Crear todos los controles
        CrearControles()

        ' Cargar puertos disponibles
        CargarPuertosDisponibles()

        ' Estado inicial
        ActualizarEstadoControles(False)
    End Sub

    Private Sub CrearControles()
        ' Título
        lblTitulo = New Label With {
            .Text = "SIMULADOR DE BÁSCULA PARA TRAILERS",
            .Font = New Font("Arial", 14, FontStyle.Bold),
            .Location = New Point(20, 10),
            .Size = New Size(550, 30),
            .TextAlign = ContentAlignment.MiddleCenter,
            .ForeColor = Color.DarkBlue
        }
        Me.Controls.Add(lblTitulo)

        ' Panel de información principal
        Dim panelInfo As New Panel With {
            .Location = New Point(20, 50),
            .Size = New Size(550, 100),
            .BorderStyle = BorderStyle.FixedSingle,
            .BackColor = Color.FromArgb(240, 240, 255)
        }

        lblPesoActual = New Label With {
            .Text = "Peso Actual:",
            .Font = New Font("Arial", 12, FontStyle.Bold),
            .Location = New Point(10, 15),
            .Size = New Size(150, 25)
        }

        lblPesoValor = New Label With {
            .Text = "0 kg",
            .Font = New Font("Arial", 24, FontStyle.Bold),
            .Location = New Point(10, 40),
            .Size = New Size(250, 50),
            .ForeColor = Color.DarkGreen
        }

        lblEstado = New Label With {
            .Text = "Estado:",
            .Font = New Font("Arial", 10, FontStyle.Bold),
            .Location = New Point(300, 15),
            .Size = New Size(80, 25)
        }

        lblEstadoValor = New Label With {
            .Text = "DETENIDO",
            .Font = New Font("Arial", 12, FontStyle.Bold),
            .Location = New Point(300, 40),
            .Size = New Size(230, 25),
            .ForeColor = Color.Gray
        }

        lblUltimoEnvio = New Label With {
            .Text = "Último envío:",
            .Font = New Font("Arial", 8),
            .Location = New Point(300, 65),
            .Size = New Size(80, 20)
        }

        lblUltimoEnvioValor = New Label With {
            .Text = "--: --:--",
            .Font = New Font("Arial", 8),
            .Location = New Point(380, 65),
            .Size = New Size(150, 20),
            .ForeColor = Color.DarkGray
        }

        panelInfo.Controls.AddRange({lblPesoActual, lblPesoValor, lblEstado, lblEstadoValor, lblUltimoEnvio, lblUltimoEnvioValor})
        Me.Controls.Add(panelInfo)

        ' Grupo Configuración
        grpConfiguracion = New GroupBox With {
            .Text = "Configuración",
            .Location = New Point(20, 160),
            .Size = New Size(270, 150)
        }

        lblPuerto = New Label With {.Text = "Puerto:", .Location = New Point(10, 25), .Size = New Size(80, 20)}
        cboPuerto = New ComboBox With {
            .Location = New Point(100, 23),
            .Size = New Size(150, 25),
            .DropDownStyle = ComboBoxStyle.DropDownList
        }

        lblIntervalo = New Label With {.Text = "Intervalo (ms):", .Location = New Point(10, 55), .Size = New Size(85, 20)}
        numIntervalo = New NumericUpDown With {
            .Location = New Point(100, 53),
            .Size = New Size(150, 25),
            .Minimum = 100,
            .Maximum = 10000,
            .Value = 1000,
            .Increment = 100
        }

        lblPesoMin = New Label With {.Text = "Peso Mín (kg):", .Location = New Point(10, 85), .Size = New Size(85, 20)}
        numPesoMin = New NumericUpDown With {
            .Location = New Point(100, 83),
            .Size = New Size(150, 25),
            .Minimum = 0,
            .Maximum = 100000,
            .Value = 0,
            .Increment = 1000
        }

        lblPesoMax = New Label With {.Text = "Peso Máx (kg):", .Location = New Point(10, 115), .Size = New Size(85, 20)}
        numPesoMax = New NumericUpDown With {
            .Location = New Point(100, 113),
            .Size = New Size(150, 25),
            .Minimum = 0,
            .Maximum = 100000,
            .Value = 50000,
            .Increment = 1000
        }

        grpConfiguracion.Controls.AddRange({lblPuerto, cboPuerto, lblIntervalo, numIntervalo, lblPesoMin, numPesoMin, lblPesoMax, numPesoMax})
        Me.Controls.Add(grpConfiguracion)

        ' Grupo Control
        grpControl = New GroupBox With {
            .Text = "Control de Simulación",
            .Location = New Point(300, 160),
            .Size = New Size(270, 150)
        }

        btnIniciar = New Button With {
            .Text = "▶ INICIAR",
            .Location = New Point(15, 25),
            .Size = New Size(110, 50),
            .BackColor = Color.LightGreen,
            .Font = New Font("Arial", 10, FontStyle.Bold)
        }
        AddHandler btnIniciar.Click, AddressOf btnIniciar_Click

        btnDetener = New Button With {
            .Text = "⏹ DETENER",
            .Location = New Point(140, 25),
            .Size = New Size(110, 50),
            .BackColor = Color.LightCoral,
            .Font = New Font("Arial", 10, FontStyle.Bold),
            .Enabled = False
        }
        AddHandler btnDetener.Click, AddressOf btnDetener_Click

        btnPausar = New Button With {
            .Text = "⏸ PAUSAR",
            .Location = New Point(15, 85),
            .Size = New Size(110, 40),
            .Enabled = False
        }
        AddHandler btnPausar.Click, AddressOf btnPausar_Click

        btnTara = New Button With {
            .Text = "⚖ TARA (0 kg)",
            .Location = New Point(140, 85),
            .Size = New Size(110, 40),
            .Enabled = False
        }
        AddHandler btnTara.Click, AddressOf btnTara_Click

        grpControl.Controls.AddRange({btnIniciar, btnDetener, btnPausar, btnTara})
        Me.Controls.Add(grpControl)

        ' Grupo Modos
        grpModos = New GroupBox With {
            .Text = "Modo de Simulación",
            .Location = New Point(20, 320),
            .Size = New Size(270, 130)
        }

        rbAleatorio = New RadioButton With {.Text = "Aleatorio (variación constante)", .Location = New Point(10, 25), .Size = New Size(240, 20)}
        rbEstable = New RadioButton With {.Text = "Estable (peso fijo)", .Location = New Point(10, 50), .Size = New Size(240, 20)}
        rbRealista = New RadioButton With {.Text = "Realista (estabilización gradual)", .Location = New Point(10, 75), .Size = New Size(240, 20), .Checked = True}
        rbIncremental = New RadioButton With {.Text = "Incremental (carga/descarga)", .Location = New Point(10, 100), .Size = New Size(240, 20)}

        AddHandler rbAleatorio.CheckedChanged, AddressOf ModoSimulacion_Changed
        AddHandler rbEstable.CheckedChanged, AddressOf ModoSimulacion_Changed
        AddHandler rbRealista.CheckedChanged, AddressOf ModoSimulacion_Changed
        AddHandler rbIncremental.CheckedChanged, AddressOf ModoSimulacion_Changed

        grpModos.Controls.AddRange({rbAleatorio, rbEstable, rbRealista, rbIncremental})
        Me.Controls.Add(grpModos)

        ' Grupo Acciones
        grpAcciones = New GroupBox With {
            .Text = "Acciones Rápidas",
            .Location = New Point(300, 320),
            .Size = New Size(270, 130)
        }

        btnSimularCarga = New Button With {
            .Text = "📦 Simular Carga Trailer",
            .Location = New Point(15, 25),
            .Size = New Size(235, 30),
            .Enabled = False
        }
        AddHandler btnSimularCarga.Click, AddressOf btnSimularCarga_Click

        btnSimularDescarga = New Button With {
            .Text = "📤 Simular Descarga Trailer",
            .Location = New Point(15, 60),
            .Size = New Size(235, 30),
            .Enabled = False
        }
        AddHandler btnSimularDescarga.Click, AddressOf btnSimularDescarga_Click

        txtPesoManual = New TextBox With {
            .Location = New Point(15, 95),
            .Size = New Size(110, 25),
            .Text = "25000",
            .TextAlign = HorizontalAlignment.Right
        }

        btnPesoManual = New Button With {
            .Text = "Establecer Peso",
            .Location = New Point(130, 95),
            .Size = New Size(120, 25),
            .Enabled = False
        }
        AddHandler btnPesoManual.Click, AddressOf btnPesoManual_Click

        grpAcciones.Controls.AddRange({btnSimularCarga, btnSimularDescarga, txtPesoManual, btnPesoManual})
        Me.Controls.Add(grpAcciones)

        ' Grupo Log
        grpLog = New GroupBox With {
            .Text = "Registro de Eventos",
            .Location = New Point(20, 460),
            .Size = New Size(550, 180)
        }

        txtLog = New TextBox With {
            .Location = New Point(10, 20),
            .Size = New Size(530, 120),
            .Multiline = True,
            .ScrollBars = ScrollBars.Vertical,
            .ReadOnly = True,
            .BackColor = Color.White,
            .Font = New Font("Consolas", 8)
        }

        btnLimpiarLog = New Button With {
            .Text = "Limpiar Log",
            .Location = New Point(440, 145),
            .Size = New Size(100, 25)
        }
        AddHandler btnLimpiarLog.Click, Sub() txtLog.Clear()

        grpLog.Controls.AddRange({txtLog, btnLimpiarLog})
        Me.Controls.Add(grpLog)
    End Sub

    Private Sub FormSimuladorBascula_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If simulador IsNot Nothing AndAlso simulador.EstaEjecutando Then
            simulador.Detener()
            simulador.Dispose()
        End If
    End Sub

    Private Sub CargarPuertosDisponibles()
        cboPuerto.Items.Clear()

        ' Agregar puertos detectados
        Dim puertos As String() = SerialPort.GetPortNames()
        For Each puerto In puertos
            cboPuerto.Items.Add(puerto)
        Next

        ' Si no hay puertos, agregar COM7 por defecto
        If cboPuerto.Items.Count = 0 Then
            cboPuerto.Items.Add("COM7")
            AgregarLog("⚠ No se detectaron puertos.  Usando COM7 por defecto.")
        End If

        ' Seleccionar COM7 si existe
        Dim indexCOM7 = cboPuerto.Items.IndexOf("COM7")
        If indexCOM7 >= 0 Then
            cboPuerto.SelectedIndex = indexCOM7
        Else
            cboPuerto.SelectedIndex = 0
        End If
    End Sub

    Private Sub ActualizarEstadoControles(enEjecucion As Boolean)
        ' Configuración (solo editable cuando está detenido)
        cboPuerto.Enabled = Not enEjecucion
        numIntervalo.Enabled = Not enEjecucion
        numPesoMin.Enabled = Not enEjecucion
        numPesoMax.Enabled = Not enEjecucion

        ' Botones de control
        btnIniciar.Enabled = Not enEjecucion
        btnDetener.Enabled = enEjecucion
        btnPausar.Enabled = enEjecucion
        btnTara.Enabled = enEjecucion

        ' Acciones
        btnSimularCarga.Enabled = enEjecucion
        btnSimularDescarga.Enabled = enEjecucion
        btnPesoManual.Enabled = enEjecucion

        ' Modos
        rbAleatorio.Enabled = enEjecucion
        rbEstable.Enabled = enEjecucion
        rbRealista.Enabled = enEjecucion
        rbIncremental.Enabled = enEjecucion
    End Sub

    Private Sub AgregarLog(mensaje As String)
        If txtLog.InvokeRequired Then
            txtLog.Invoke(Sub() AgregarLog(mensaje))
            Return
        End If

        Dim hora As String = DateTime.Now.ToString("HH:mm:ss")
        txtLog.AppendText($"[{hora}] {mensaje}{Environment.NewLine}")

        ' Auto-scroll al final
        txtLog.SelectionStart = txtLog.Text.Length
        txtLog.ScrollToCaret()

        ' Limitar líneas (mantener últimas 100)
        Dim lineas = txtLog.Lines
        If lineas.Length > 100 Then
            txtLog.Lines = lineas.Skip(lineas.Length - 100).ToArray()
        End If
    End Sub

    Private Sub ActualizarPesoUI(peso As Decimal)
        If lblPesoValor.InvokeRequired Then
            lblPesoValor.Invoke(Sub() ActualizarPesoUI(peso))
            Return
        End If

        lblPesoValor.Text = $"{CInt(peso):N0} kg"

        ' Cambiar color según el peso
        If peso = 0 Then
            lblPesoValor.ForeColor = Color.Gray
        ElseIf peso < 10000 Then
            lblPesoValor.ForeColor = Color.DarkGreen
        ElseIf peso < 30000 Then
            lblPesoValor.ForeColor = Color.DarkOrange
        Else
            lblPesoValor.ForeColor = Color.DarkRed
        End If
    End Sub

    Private Sub ActualizarEstadoUI(estado As String, color As Color)
        If lblEstadoValor.InvokeRequired Then
            lblEstadoValor.Invoke(Sub() ActualizarEstadoUI(estado, color))
            Return
        End If

        lblEstadoValor.Text = estado
        lblEstadoValor.ForeColor = color
    End Sub

    Private Sub btnIniciar_Click(sender As Object, e As EventArgs)
        Try
            ' Crear simulador
            Dim puerto As String = cboPuerto.SelectedItem.ToString()
            simulador = New SimuladorBascula(puerto)

            ' Configurar
            simulador.IntervaloEnvio = CInt(numIntervalo.Value)
            simulador.PesoMinimo = numPesoMin.Value
            simulador.PesoMaximo = numPesoMax.Value

            ' Establecer modo inicial
            If rbAleatorio.Checked Then
                simulador.Modo = SimuladorBascula.ModoSimulacion.Aleatorio
            ElseIf rbEstable.Checked Then
                simulador.Modo = SimuladorBascula.ModoSimulacion.Estable
            ElseIf rbRealista.Checked Then
                simulador.Modo = SimuladorBascula.ModoSimulacion.Realista
            ElseIf rbIncremental.Checked Then
                simulador.Modo = SimuladorBascula.ModoSimulacion.Incremental
            End If

            ' Suscribir a eventos
            AddHandler simulador.DatosEnviados, AddressOf Simulador_DatosEnviados
            AddHandler simulador.ErrorOcurrido, AddressOf Simulador_ErrorOcurrido
            AddHandler simulador.ConexionCambiada, AddressOf Simulador_ConexionCambiada

            ' Iniciar
            simulador.Iniciar()

            ActualizarEstadoControles(True)
            ActualizarEstadoUI("EJECUTANDO", Color.DarkGreen)
            AgregarLog($"✓ Simulador iniciado en {puerto}")

        Catch ex As Exception
            MessageBox.Show($"Error al iniciar simulador:{Environment.NewLine}{ex.Message}",
                          "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            AgregarLog($"❌ Error:  {ex.Message}")
        End Try
    End Sub

    Private Sub btnDetener_Click(sender As Object, e As EventArgs)
        If simulador IsNot Nothing Then
            simulador.Detener()
            simulador.Dispose()
            simulador = Nothing
        End If

        ActualizarEstadoControles(False)
        ActualizarEstadoUI("DETENIDO", Color.Gray)
        lblPesoValor.Text = "0 kg"
        lblPesoValor.ForeColor = Color.Gray
        lblUltimoEnvioValor.Text = "--: --: --"
        AgregarLog("⏹ Simulador detenido")
    End Sub

    Private Sub btnPausar_Click(sender As Object, e As EventArgs)
        If simulador Is Nothing Then Return

        If btnPausar.Text.Contains("PAUSAR") Then
            simulador.Pausar()
            btnPausar.Text = "▶ REANUDAR"
            ActualizarEstadoUI("PAUSADO", Color.DarkOrange)
            AgregarLog("⏸ Simulación pausada")
        Else
            simulador.Reanudar()
            btnPausar.Text = "⏸ PAUSAR"
            ActualizarEstadoUI("EJECUTANDO", Color.DarkGreen)
            AgregarLog("▶ Simulación reanudada")
        End If
    End Sub

    Private Sub btnTara_Click(sender As Object, e As EventArgs)
        If simulador IsNot Nothing Then
            simulador.AplicarTara()
            AgregarLog("⚖ Tara aplicada - Peso establecido en 0 kg")
        End If
    End Sub

    Private Sub btnSimularCarga_Click(sender As Object, e As EventArgs)
        If simulador IsNot Nothing Then
            simulador.SimularCarga(0, numPesoMax.Value, 30)
            rbIncremental.Checked = True
            AgregarLog($"📦 Simulando carga de trailer:  0 → {numPesoMax.Value} kg")
        End If
    End Sub

    Private Sub btnSimularDescarga_Click(sender As Object, e As EventArgs)
        If simulador IsNot Nothing Then
            simulador.SimularDescarga(20)
            rbIncremental.Checked = True
            AgregarLog("📤 Simulando descarga de trailer")
        End If
    End Sub

    Private Sub btnPesoManual_Click(sender As Object, e As EventArgs)
        If simulador Is Nothing Then Return

        Dim peso As Decimal
        If Decimal.TryParse(txtPesoManual.Text, peso) Then
            simulador.EstablecerPeso(peso)
            rbEstable.Checked = True
            AgregarLog($"⚖ Peso manual establecido:  {peso} kg")
        Else
            MessageBox.Show("Ingresa un peso válido", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End If
    End Sub

    Private Sub ModoSimulacion_Changed(sender As Object, e As EventArgs)
        If simulador Is Nothing OrElse Not DirectCast(sender, RadioButton).Checked Then Return

        If rbAleatorio.Checked Then
            simulador.Modo = SimuladorBascula.ModoSimulacion.Aleatorio
            AgregarLog("🎲 Modo cambiado:  ALEATORIO")
        ElseIf rbEstable.Checked Then
            simulador.Modo = SimuladorBascula.ModoSimulacion.Estable
            AgregarLog("📌 Modo cambiado:  ESTABLE")
        ElseIf rbRealista.Checked Then
            simulador.Modo = SimuladorBascula.ModoSimulacion.Realista
            AgregarLog("🎯 Modo cambiado:  REALISTA")
        ElseIf rbIncremental.Checked Then
            simulador.Modo = SimuladorBascula.ModoSimulacion.Incremental
            AgregarLog("📈 Modo cambiado:  INCREMENTAL")
        End If
    End Sub

    Private Sub Simulador_DatosEnviados(mensaje As String)
        If Me.IsDisposed OrElse lblUltimoEnvioValor Is Nothing Then Return

        lblUltimoEnvioValor.Invoke(Sub() lblUltimoEnvioValor.Text = DateTime.Now.ToString("HH: mm:ss"))

        If simulador IsNot Nothing Then
            ActualizarPesoUI(simulador.PesoActual)
        End If

        ' Solo loguear cada 5 envíos para no saturar
        Static contador As Integer = 0
        contador += 1
        If contador Mod 5 = 0 Then
            AgregarLog($"{mensaje}")
        End If
    End Sub

    Private Sub Simulador_ErrorOcurrido(mensaje As String)
        AgregarLog($"❌ ERROR: {mensaje}")
        ActualizarEstadoUI("ERROR", Color.Red)
    End Sub

    Private Sub Simulador_ConexionCambiada(conectado As Boolean)
        If conectado Then
            AgregarLog("✓ Puerto conectado correctamente")
        Else
            AgregarLog("⚠ Puerto desconectado")
        End If
    End Sub

End Class