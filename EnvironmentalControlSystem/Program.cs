using System;
using System.Threading; // Diperlukan untuk Thread.Sleep

// --- 1. Struktur Data Sensor ---
public struct SensorData
{
    public bool S1_HeartRate { get; set; } // H
    public bool S2_PulseRate { get; set; } // P
    public bool S3_Oksigen { get; set; }   // O
    public bool S4_Suhu { get; set; }      // T
    public bool S5_Respiration { get; set; }// R
    public bool S6_BloodPressure { get; set; } // B
}

// --- 2. Enum untuk Status FSM ---
public enum State
{
    S0_Normal,
    S1_Peringatan,
    S2_KrisisTotal,
    S3_Unused // S3 (11) di tabel Verilog.
}

// --- 3. Kelas FSM Utama ---
public class VitalSignFSM
{
    private State _currentState;
    private SensorData _currentSensors;

    public VitalSignFSM()
    {
        _currentState = State.S0_Normal;
        Console.WriteLine("FSM Initialized. Current State: S0_Normal");
    }

    public void UpdateSensors(SensorData sensors)
    {
        _currentSensors = sensors;
    }

    // --- Logika Output Aktuator ---
    private string GetActuatorStatus(bool A, bool C_Total)
    {
        // Global Actuators
        bool Buzzer = C_Total;
        bool Visual = A; // LED & Vibrator

        // Mitigasi Actuators (hanya aktif jika A=true)
        bool MicroFan = A && _currentSensors.S4_Suhu; // Fan dipicu oleh T
        bool SolenoidValve = A && _currentSensors.S3_Oksigen; // SV dipicu oleh O
        bool InfusePump = A && _currentSensors.S6_BloodPressure; // Pump dipicu oleh B

        return $"Aktuator: Buzzer={Buzzer.ToBinary()}, LED/Vibrator={Visual.ToBinary()}, " +
               $"Fan={MicroFan.ToBinary()}, SV={SolenoidValve.ToBinary()}, Pump={InfusePump.ToBinary()}";
    }

    // --- Logika Utama FSM: Transisi dan Eksekusi ---
    public void Execute()
    {
        // Derived Inputs
        bool A = _currentSensors.S1_HeartRate || _currentSensors.S2_PulseRate || _currentSensors.S3_Oksigen ||
                 _currentSensors.S4_Suhu || _currentSensors.S5_Respiration || _currentSensors.S6_BloodPressure;

        bool C_Total = _currentSensors.S1_HeartRate && _currentSensors.S2_PulseRate && _currentSensors.S3_Oksigen &&
                       _currentSensors.S4_Suhu && _currentSensors.S5_Respiration && _currentSensors.S6_BloodPressure;

        State nextState = _currentState;

        switch (_currentState)
        {
            case State.S0_Normal:
                if (C_Total) nextState = State.S2_KrisisTotal;
                else if (A) nextState = State.S1_Peringatan;
                // else nextState remains S0
                break;
            case State.S1_Peringatan:
                if (C_Total) nextState = State.S2_KrisisTotal;
                else if (!A) nextState = State.S0_Normal;
                // else nextState remains S1
                break;
            case State.S2_KrisisTotal:
                if (!A) nextState = State.S0_Normal;
                else if (!C_Total) nextState = State.S1_Peringatan;
                // else nextState remains S2
                break;
        }

        Console.WriteLine($"  Status A/C_Total: {A.ToBinary()}/{C_Total.ToBinary()}");
        Console.WriteLine($"  Transisi: {_currentState} -> {nextState}");
        _currentState = nextState;
        Console.WriteLine($"  {GetActuatorStatus(A, C_Total)}");
        Console.WriteLine("------------------------------------------");
    }

    public string GetStatus()
    {
        return $"[CURRENT STATE]: {_currentState}";
    }
}

// --- 4. Kelas Extension untuk Konversi Bool ke 0/1 ---
public static class BoolExtension
{
    public static string ToBinary(this bool value)
    {
        return value ? "1" : "0";
    }
}

// --- 5. Main Program ---
public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("--- SIMULASI FSM VITAL SIGN C# ---");
        VitalSignFSM fsm = new VitalSignFSM();
        SensorData sensors = new SensorData();

        // Jeda untuk output yang lebih mudah dibaca
        void SleepAndPrintStatus(VitalSignFSM fsm)
        {
            Console.WriteLine(fsm.GetStatus());
            Thread.Sleep(500); // 500ms
        }

        // ==========================================================
        // Test Case 1: Start (S0) -> S1 (Risiko Peringatan: H)
        // ==========================================================
        Console.WriteLine("\nTEST 1: S0 -> S1 (H-Risk)");
        sensors = new SensorData { S1_HeartRate = true, S4_Suhu = false }; // A=1, C=0
        fsm.UpdateSensors(sensors);
        fsm.Execute();
        SleepAndPrintStatus(fsm);

        // ==========================================================
        // Test Case 2: S1 -> S2 (Risiko Krisis Total: Semua ON)
        // ==========================================================
        Console.WriteLine("\nTEST 2: S1 -> S2 (Semua-Risk)");
        sensors = new SensorData
        {
            S1_HeartRate = true,
            S2_PulseRate = true,
            S3_Oksigen = true,
            S4_Suhu = true,
            S5_Respiration = true,
            S6_BloodPressure = true
        }; // A=1, C=1
        fsm.UpdateSensors(sensors);
        fsm.Execute();
        SleepAndPrintStatus(fsm);

        // ==========================================================
        // Test Case 3: S2 -> S1 (Risiko Peringatan: T, O, R Risk)
        // ==========================================================
        Console.WriteLine("\nTEST 3: S2 -> S1 (T, O, R - Risk)");
        sensors = new SensorData
        {
            S1_HeartRate = false,
            S2_PulseRate = false,
            S3_Oksigen = true,
            S4_Suhu = true,
            S5_Respiration = true,
            S6_BloodPressure = false
        }; // A=1, C=0
        fsm.UpdateSensors(sensors);
        fsm.Execute();
        SleepAndPrintStatus(fsm);

        // ==========================================================
        // Test Case 4: S1 -> S0 (Semua Aman)
        // ==========================================================
        Console.WriteLine("\nTEST 4: S1 -> S0 (Semua Aman)");
        sensors = new SensorData
        {
            S1_HeartRate = false,
            S2_PulseRate = false,
            S3_Oksigen = false,
            S4_Suhu = false,
            S5_Respiration = false,
            S6_BloodPressure = false
        }; // A=0, C=0
        fsm.UpdateSensors(sensors);
        fsm.Execute();
        SleepAndPrintStatus(fsm);

        // ==========================================================
        // Test Case 5: S0 -> S2 (Lompat ke Krisis Total)
        // ==========================================================
        Console.WriteLine("\nTEST 5: S0 -> S2 (Semua-Risk)");
        sensors = new SensorData
        {
            S1_HeartRate = true,
            S2_PulseRate = true,
            S3_Oksigen = true,
            S4_Suhu = true,
            S5_Respiration = true,
            S6_BloodPressure = true
        }; // A=1, C=1
        fsm.UpdateSensors(sensors);
        fsm.Execute();
        SleepAndPrintStatus(fsm);

        // ==========================================================
        // Test Case 6: S2 -> S0 (Exit Krisis Total ke Normal)
        // ==========================================================
        Console.WriteLine("\nTEST 6: S2 -> S0 (Semua Aman)");
        sensors = new SensorData
        {
            S1_HeartRate = false,
            S2_PulseRate = false,
            S3_Oksigen = false,
            S4_Suhu = false,
            S5_Respiration = false,
            S6_BloodPressure = false
        }; // A=0, C=0
        fsm.UpdateSensors(sensors);
        fsm.Execute();
        SleepAndPrintStatus(fsm);

        // ==========================================================
        // Test Case Custom (Sesuai Hal 8)
        // ==========================================================
        Console.WriteLine("\nTEST CUSTOM: T+O Abnormal -> S1");
        // Di sini saya asumsikan VOC+Dust abnormal berarti T+O Risk.
        sensors.S1_HeartRate = true; sensors.S2_PulseRate = true;
        sensors.S3_Oksigen = true; sensors.S4_Suhu = true;
        sensors.S5_Respiration = true; sensors.S6_BloodPressure = false; // A=1, C=0
        fsm.UpdateSensors(sensors);
        fsm.Execute();
        SleepAndPrintStatus(fsm);

        // PENTING: Gunakan Console.ReadKey() agar console tidak langsung tertutup
        // Seperti yang disarankan dalam TIPS PENTING (Hal 11)
        Console.WriteLine("\nSimulasi Selesai. Tekan tombol apa saja untuk menutup.");
        Console.ReadKey();
    }
}