`timescale 1ns/1ps

module tb_environmental_control;

// --- Deklarasi Sinyal ---
reg clk;
reg reset;
reg H, P, O, T, R, B; // Sensor Inputs (S1-S6)
wire Buzzer, LED, Vibrator, Micro_Fan, Selenoid_Valve, Infuse_Pump; // Actuator Outputs (A1-A6)

// --- Instansiasi Unit Under Test (UUT) ---
fsm_vital_sign uut (
    .clk(clk),
    .reset(reset),
    .H(H), .P(P), .O(O), .T(T), .R(R), .B(B), // Input Sensor
    .Buzzer(Buzzer), .LED(LED), .Vibrator(Vibrator), 
    .Micro_Fan(Micro_Fan), .Selenoid_Valve(Selenoid_Valve), .Infuse_Pump(Infuse_Pump) // Output Aktuator
);

// --- Clock Generator ---
initial begin
    clk = 0;
    forever #20 clk = ~clk; // Periode clock 40ns (Frekuensi 25MHz)
end

// --- Setup Dumpfile (Untuk GTKWave) ---
initial begin
    $dumpfile("environmental_control.vcd");
    $dumpvars(0, tb_environmental_control);
end

// --- Sinyal Stimulus Test Sequence (S0 -> S1 -> S2 -> S1 -> S0) ---
initial begin
    // 1. Inisialisasi
    reset = 1; #50; // Jaga reset tetap tinggi beberapa waktu
    reset = 0; #50; // Reset Selesai. Status: S0 (Normal). Sensor: 000000. Aktuator: 000000.

    // 2. Transisi S0 -> S1 (Risiko Peringatan: H-Risk ON)
    H = 1; P = 0; O = 0; T = 0; R = 0; B = 0; // A=1, C_Total=0
    #80; // Status: S1 (Peringatan). Aktuator: Buzzer=0, Visual=1, Mitigasi=0 (karena T,O,B=0).

    // 3. Transisi S1 -> S2 (Risiko Krisis Total: Semua ON)
    H = 1; P = 1; O = 1; T = 1; R = 1; B = 1; // A=1, C_Total=1
    #80; // Status: S2 (Krisis Total). Aktuator: Buzzer=1, Visual=1, Mitigasi=1.

    // 4. Transisi S2 -> S1 (Risiko Peringatan: T, O, R Risk ON)
    H = 0; P = 0; O = 1; T = 1; R = 1; B = 0; // A=1, C_Total=0
    #80; // Status: S1 (Peringatan). Aktuator: Buzzer=0, Visual=1, Mitigasi=1 (Fan=1, SV=1, Pump=0).

    // 5. Transisi S1 -> S0 (Semua Aman)
    H = 0; P = 0; O = 0; T = 0; R = 0; B = 0; // A=0, C_Total=0
    #80; // Status: S0 (Normal). Aktuator: 000000.

    $finish;
end

endmodule