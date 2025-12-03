# Formal Modeling and Verilog Simulation of a Deterministic Vital Sign Monitoring System: A Quantum-Inspired FSM Approach

Proyek ini adalah implementasi **Finite State Machine (FSM)** untuk sistem pemantauan tanda vital (*vital sign*) pasien. Sistem ini memantau parameter seperti detak jantung, denyut nadi, oksigen, suhu, pernapasan, dan tekanan darah untuk mendeteksi tingkat risiko dan mengaktifkan aktuator yang sesuai secara otomatis.

Implementasi proyek mencakup dua bagian utama:
1.  **Hardware Description (Verilog):** Desain logis FSM dan testbench untuk simulasi digital.
2.  **Software Simulation (C#):** Simulasi logika FSM untuk demonstrasi perangkat lunak dan debugging logika.

## 🧠 Logika FSM

Sistem beroperasi berdasarkan 3 status utama (State):

| State | Deskripsi | Kondisi | Aktuator Aktif (Contoh) |
| :--- | :--- | :--- | :--- |
| **S0 (Normal)** | Pasien dalam kondisi aman. | Tidak ada risiko ($A=0, C_{Total}=0$) | Tidak ada (Semua OFF) |
| **S1 (Peringatan)** | Terdeteksi risiko parsial. | Salah satu sensor bahaya ($A=1, C_{Total}=0$) | LED, Vibrator |
| **S2 (Krisis Total)** | Pasien dalam kondisi kritis. | Semua sensor bahaya ($A=1, C_{Total}=1$) | Buzzer, Fan, Infuse Pump, dll |

**Input Sensor:**
*   **H, P, O, T, R, B:** Input dari sensor vital.
*   **A (Any Risk):** Logika OR dari semua sensor.
*   **C_Total (Critical Risk):** Logika AND dari semua sensor.

## ✨ Fitur

*   **Deteksi Input Sensor:** Memproses input dari berbagai sensor vital sign.
*   **Logika Transisi Cerdas:** Berpindah antar status S0, S1, dan S2 secara otomatis berdasarkan kondisi pasien.
*   **Kontrol Aktuator:** Mengaktifkan Buzzer, LED, Vibrator, Fan, Solenoid Valve, dan Infuse Pump sesuai urgensi.
*   **Visualisasi Waveform:** Simulasi Verilog menghasilkan file VCD untuk dianalisis di GTKWave.
*   **Simulasi Console:** Output real-time di terminal menggunakan C#.

## 📋 Prasyarat

Pastikan Anda telah menginstal tools berikut sebelum menjalankan proyek:

**Untuk Simulasi Verilog:**
*   [Icarus Verilog](http://iverilog.icarus.com/) (Kompilasi dan Simulasi)
*   [GTKWave](http://gtkwave.sourceforge.net/) (Visualisasi Waveform)

**Untuk Simulasi C#:**
*   [.NET SDK](https://dotnet.microsoft.com/download) (Versi 6.0 atau lebih tinggi)

**Sistem Operasi:**
*   Windows / Linux / macOS (Diuji pada Windows dengan Icarus Verilog).

## 🚀 Instalasi

1.  **Clone Repositori:**
    ```bash
    git clone https://github.com/username/repo.git
    cd repo
    ```

2.  Pastikan semua *prerequisites* sudah terinstal dan terdaftar di PATH sistem Anda.

## 💻 Cara Menjalankan

### 1. Simulasi Verilog (Hardware)

Langkah ini akan mengkompilasi kode Verilog dan menghasilkan waveform.

1.  Kompilasi dan jalankan testbench:
    ```bash
    iverilog -o simulation environmental_control.v tb_environmental_control.v
    vvp simulation
    ```
2.  Perintah di atas akan menghasilkan file `environmental_control.vcd`. Buka file tersebut dengan GTKWave:
    ```bash
    gtkwave environmental_control.vcd
    ```
3.  Di dalam GTKWave, tarik sinyal (seperti `current_state`, input sensor, output aktuator) ke timeline untuk melihat transisi status.

### 2. Simulasi C# (Software)

Langkah ini menjalankan simulasi logika di terminal.

1.  Siapkan project C# (jika belum ada folder build):
    ```bash
    dotnet new console -o csharp_sim
    # Salin file source code ke folder project
    # (Sesuaikan perintah copy jika menggunakan Linux/Mac: cp Program.cs csharp_sim/Program.cs)
    copy Program.cs csharp_sim\Program.cs
    cd csharp_sim
    ```
2.  Jalankan program:
    ```bash
    dotnet run
    ```
3.  Output akan muncul di terminal menunjukkan transisi status dan aktuator yang aktif.

## 📊 Contoh Output

### C# Console Output
```text
--- SIMULASI FSM VITAL SIGN C# ---
FSM Initialized. Current State: S0_Normal

TEST 1: S0 -> S1 (H-Risk)
  Status A/C_Total: 1/0
  Transisi: S0_Normal -> S1_Peringatan
  Aktuator: Buzzer=0, LED/Vibrator=1, Fan=0, SV=0, Pump=0
[CURRENT STATE]: S1_Peringatan
...
```
### Verilog Waveform
Visualisasi pada GTKWave akan menunjukkan sinyal biner `00` (S0) berubah menjadi `01` (S1) ketika salah satu input sensor bernilai High (`1`).

## 📂 Struktur File

*   `environmental_control.v`: Modul utama FSM dalam bahasa Verilog.
*   `tb_environmental_control.v`: Testbench untuk memberikan stimulus input pada simulasi Verilog.
*   `environmental_control.vcd`: File output waveform (digenerate setelah simulasi).
*   `Program.cs`: Implementasi logika FSM dan test case menggunakan C#.
*   `simulation`: File output biner dari Icarus Verilog.
