Imports System.IO.Ports
Imports System.Threading

Public Class SimuladorBascula
    Implements IDisposable

    ' Puerto serial
    Private WithEvents serialPort As SerialPort

    ' Control de threads
    Private simulationThread As Thread
    Private isRunning As Boolean = False
    Private pauseSimulation As Boolean = False

    ' Variables de peso
    Private currentWeight As Decimal = 0
    Private targetWeight As Decimal = 0
    Private random As New Random()

    ' *** VARIABLES PRIVADAS DE RESPALDO (backing fields) ***
    Private _intervaloEnvio As Integer = 1000
    Private _pesoMinimo As Decimal = 0
    Private _pesoMaximo As Decimal = 50000
    Private _variacionMaxima As Decimal = 500
    Private _modoActual As ModoSimulacion = ModoSimulacion.Aleatorio

    ' Modos de simulación
    Public Enum ModoSimulacion
        Aleatorio
        Incremental
        Estable
        Oscilante
        Realista
    End Enum

    ' Eventos
    Public Event DatosEnviados(mensaje As String)
    Public Event ErrorOcurrido(mensaje As String)
    Public Event ConexionCambiada(conectado As Boolean)

#Region "Constructor y Configuración"

    Public Sub New(nombrePuerto As String)
        ConfigurarPuerto(nombrePuerto, 9600, Parity.None, 8, StopBits.One)
    End Sub

    Public Sub New(nombrePuerto As String, baudRate As Integer, parity As Parity, dataBits As Integer, stopBits As StopBits)
        ConfigurarPuerto(nombrePuerto, baudRate, parity, dataBits, stopBits)
    End Sub

    Private Sub ConfigurarPuerto(nombrePuerto As String, baudRate As Integer, parity As Parity, dataBits As Integer, stopBits As StopBits)
        serialPort = New SerialPort()
        serialPort.PortName = nombrePuerto
        serialPort.BaudRate = baudRate
        serialPort.DataBits = dataBits
        serialPort.Parity = parity
        serialPort.StopBits = stopBits
        serialPort.Handshake = Handshake.None
        serialPort.ReadTimeout = 2000
        serialPort.WriteTimeout = 2000
    End Sub

#End Region

#Region "Propiedades CORREGIDAS"

    Public ReadOnly Property EstaEjecutando As Boolean
        Get
            Return isRunning
        End Get
    End Property

    Public ReadOnly Property EstaConectado As Boolean
        Get
            Return serialPort IsNot Nothing AndAlso serialPort.IsOpen
        End Get
    End Property

    Public ReadOnly Property PesoActual As Decimal
        Get
            Return currentWeight
        End Get
    End Property

    ''' <summary>
    ''' CORREGIDO: Usa variable privada _intervaloEnvio
    ''' </summary>
    Public Property IntervaloEnvio As Integer
        Get
            Return _intervaloEnvio
        End Get
        Set(value As Integer)
            If value >= 100 AndAlso value <= 10000 Then
                _intervaloEnvio = value
            End If
        End Set
    End Property

    ''' <summary>
    ''' CORREGIDO: Usa variable privada _pesoMinimo
    ''' </summary>
    Public Property PesoMinimo As Decimal
        Get
            Return _pesoMinimo
        End Get
        Set(value As Decimal)
            _pesoMinimo = value
        End Set
    End Property

    ''' <summary>
    ''' CORREGIDO:  Usa variable privada _pesoMaximo
    ''' </summary>
    Public Property PesoMaximo As Decimal
        Get
            Return _pesoMaximo
        End Get
        Set(value As Decimal)
            _pesoMaximo = value
        End Set
    End Property

    ''' <summary>
    ''' CORREGIDO:  Usa variable privada _modoActual
    ''' </summary>
    Public Property Modo As ModoSimulacion
        Get
            Return _modoActual
        End Get
        Set(value As ModoSimulacion)
            _modoActual = value
        End Set
    End Property

#End Region

#Region "Métodos Públicos"

    Public Sub Iniciar()
        If isRunning Then
            RaiseEvent ErrorOcurrido("La simulación ya está en ejecución")
            Return
        End If

        Try
            If Not serialPort.IsOpen Then
                serialPort.Open()
                RaiseEvent ConexionCambiada(True)
            End If

            isRunning = True
            pauseSimulation = False
            simulationThread = New Thread(AddressOf SimularDatos)
            simulationThread.IsBackground = True
            simulationThread.Start()

        Catch ex As Exception
            isRunning = False
            RaiseEvent ErrorOcurrido($"Error al iniciar:  {ex.Message}")
        End Try
    End Sub

    Public Sub Detener()
        isRunning = False

        If simulationThread IsNot Nothing AndAlso simulationThread.IsAlive Then
            simulationThread.Join(2000)
        End If

        If serialPort IsNot Nothing AndAlso serialPort.IsOpen Then
            serialPort.Close()
            RaiseEvent ConexionCambiada(False)
        End If
    End Sub

    Public Sub Pausar()
        pauseSimulation = True
    End Sub

    Public Sub Reanudar()
        pauseSimulation = False
    End Sub

    Public Sub EstablecerPeso(peso As Decimal)
        If peso >= _pesoMinimo AndAlso peso <= _pesoMaximo Then
            currentWeight = peso
            targetWeight = peso
        End If
    End Sub

    Public Sub SimularCarga(pesoInicial As Decimal, pesoFinal As Decimal, duracionSegundos As Integer)
        _modoActual = ModoSimulacion.Incremental
        currentWeight = pesoInicial
        targetWeight = pesoFinal
    End Sub

    Public Sub SimularDescarga(duracionSegundos As Integer)
        _modoActual = ModoSimulacion.Incremental
        targetWeight = 0
    End Sub

    Public Sub AplicarTara()
        currentWeight = 0
        targetWeight = 0
        EnviarRespuesta("OK TARA")
    End Sub

#End Region

#Region "Simulación de Datos"

    Private Sub SimularDatos()
        While isRunning
            Try
                If Not pauseSimulation AndAlso serialPort.IsOpen Then
                    ActualizarPeso()
                    Dim datos As String = FormatearDatos()
                    serialPort.Write(datos)
                    RaiseEvent DatosEnviados(datos.Trim())
                End If

                Thread.Sleep(_intervaloEnvio) ' Usa la variable privada

            Catch ex As Exception
                RaiseEvent ErrorOcurrido($"Error en simulación: {ex.Message}")
            End Try
        End While
    End Sub

    Private Sub ActualizarPeso()
        Select Case _modoActual ' Usa la variable privada
            Case ModoSimulacion.Aleatorio
                Dim variacion As Decimal = CDec(random.NextDouble() * CDbl(_variacionMaxima * 2) - CDbl(_variacionMaxima))
                currentWeight += variacion

            Case ModoSimulacion.Incremental
                If currentWeight < targetWeight Then
                    currentWeight += 100
                ElseIf currentWeight > targetWeight Then
                    currentWeight -= 100
                End If

            Case ModoSimulacion.Estable
                Dim variacion As Decimal = CDec(random.NextDouble() * 10 - 5)
                currentWeight += variacion

            Case ModoSimulacion.Oscilante
                Dim tiempo As Double = DateTime.Now.Millisecond / 1000.0
                currentWeight = targetWeight + CDec(Math.Sin(tiempo * Math.PI) * CDbl(_variacionMaxima))

            Case ModoSimulacion.Realista
                If random.Next(0, 100) < 5 Then
                    targetWeight = CDec(random.Next(CInt(_pesoMinimo), CInt(_pesoMaximo)))
                End If

                Dim diferencia As Decimal = targetWeight - currentWeight
                currentWeight += diferencia * 0.1D
                currentWeight += CDec(random.NextDouble() * 20 - 10)
        End Select

        ' Aplicar límites usando variables privadas
        If currentWeight < _pesoMinimo Then currentWeight = _pesoMinimo
        If currentWeight > _pesoMaximo Then currentWeight = _pesoMaximo
    End Sub

    Private Function FormatearDatos() As String
        Try
            ' Asegurar que currentWeight tiene valor
            If currentWeight < _pesoMinimo Then currentWeight = _pesoMinimo
            If currentWeight > _pesoMaximo Then currentWeight = _pesoMaximo

            ' Convertir a entero
            Dim pesoEntero As Integer = CInt(Math.Round(currentWeight))

            ' Formatear:  6 dígitos con ceros a la izquierda
            Dim pesoFormateado As String = pesoEntero.ToString("000000") ' Alternativa a D6

            ' Construir mensaje
            Dim mensaje As String = pesoFormateado & " kg" & vbCrLf

            Return mensaje

        Catch ex As Exception
            ' En caso de error, devolver formato de emergencia
            Return "000000 kg" & vbCrLf
        End Try
    End Function

#End Region

#Region "Manejo de Comandos"

    Private Sub serialPort_DataReceived(sender As Object, e As SerialDataReceivedEventArgs) Handles serialPort.DataReceived
        Try
            If Not serialPort.IsOpen Then Return

            Dim comando As String = serialPort.ReadExisting().Trim().ToUpper()
            If String.IsNullOrWhiteSpace(comando) Then Return

            Select Case comando
                Case "W", "WEIGHT", "P", "PESO"
                    EnviarRespuesta(FormatearDatos())

                Case "T", "TARE", "TARA"
                    AplicarTara()

                Case "Z", "ZERO", "CERO"
                    currentWeight = 0
                    targetWeight = 0
                    EnviarRespuesta("OK ZERO")

                Case "S", "STATUS", "ESTADO"
                    Dim estado As String = $"MODO:{_modoActual},PESO:{CInt(currentWeight)},MIN:{_pesoMinimo},MAX:{_pesoMaximo}"
                    EnviarRespuesta(estado & vbCrLf)

                Case Else
                    EnviarRespuesta($"ERROR: Comando desconocido '{comando}'{vbCrLf}")
            End Select

        Catch ex As Exception
            RaiseEvent ErrorOcurrido($"Error procesando comando: {ex.Message}")
        End Try
    End Sub

    Private Sub EnviarRespuesta(respuesta As String)
        Try
            If serialPort.IsOpen Then
                serialPort.Write(respuesta)
                RaiseEvent DatosEnviados($"RESPUESTA: {respuesta.Trim()}")
            End If
        Catch ex As Exception
            RaiseEvent ErrorOcurrido($"Error enviando respuesta: {ex.Message}")
        End Try
    End Sub

#End Region

#Region "IDisposable"

    Public Sub Dispose() Implements IDisposable.Dispose
        Detener()

        If serialPort IsNot Nothing Then
            serialPort.Dispose()
            serialPort = Nothing
        End If
    End Sub

#End Region

End Class