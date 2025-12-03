`timescale 1ns/1ps

module fsm_vital_sign (
    // Clock & Reset
    input clk,
    input reset,

    // 6 Sensor Inputs (1=Risk) -> S1, S2, S3, S4, S5, S6 di tutorial lama
    input H, P, O, T, R, B, 

    // 6 Actuator Outputs (1=ON) -> A1, A2, A3, A4, A5, A6 di tutorial lama
    output reg Buzzer, LED, Vibrator, Micro_Fan, Selenoid_Valve, Infuse_Pump
);

// --- 1. Definisi Status FSM (Q1Q0) ---
// S0=00, S1=01, S2=10
parameter S0 = 2'b00; // Normal
parameter S1 = 2'b01; // Peringatan
parameter S2 = 2'b10; // Krisis Total

reg [1:0] current_state;
reg [1:0] next_state;

// --- 2. Logika Input Derivasi (A & C_Total) ---
// Logika yang diambil dari Definisi 3 Status FSM (Hal 9)
wire A;      // Risiko Apapun (OR)
wire C_Total; // Risiko Kritis Total (AND)

assign A       = H | P | O | T | R | B;
assign C_Total = H & P & O & T & R & B;

// --- 3. Logika Next State (Transisi Status) ---
always @(*) begin
    next_state = current_state; // Default: Tetap di status saat ini

    case (current_state)
        S0: begin // Normal
            if (C_Total)    next_state = S2;  // Krisis Total (1,1) -> S2
            else if (A)     next_state = S1;  // Peringatan (1,0) -> S1
            else            next_state = S0;  // Aman (0,0) -> S0
        end
        S1: begin // Peringatan
            if (C_Total)    next_state = S2;  // Krisis Total (1,1) -> S2
            else if (!A)    next_state = S0;  // Kembali Normal (0,0) -> S0
            // else (A=1, C_Total=0): next_state = S1; (Default)
        end
        S2: begin // Krisis Total
            if (!A)         next_state = S0;  // Kembali Normal (0,0) -> S0
            else if (!C_Total) next_state = S1;  // Risiko Peringatan (1,0) -> S1
            // else (A=1, C_Total=1): next_state = S2; (Default)
        end
        default: next_state = S0;
    endcase
end

// --- 4. State Register (D Flip-Flop) ---
always @(posedge clk or posedge reset) begin
    if (reset) begin
        current_state <= S0;
    end else begin
        current_state <= next_state;
    end
end

// --- 5. Logika Output Aktuator ---
// Logika diambil dari Tabel Transisi Status (Hal 10)
// Buzzer = C_Total (Ketika S_next=S2)
// Visual (LED/Vibrator) = A (Ketika S_next=S1 atau S2)
// Mitigasi = A (Ketika S_next=S1 atau S2)

always @(*) begin
    // Aktuator Visual & Buzzer (Global)
    Buzzer = C_Total; // O_Buzzer = C_Total
    LED = A;          // O_Visual = A
    Vibrator = A;     // O_Visual = A

    // Aktuator Mitigasi (Individual, dipicu saat A=1)
    // Logika individual untuk Mitigasi (Fan=T, SV=O, Pump=B) hanya aktif di status S1 atau S2 (A=1)
    if (A) begin
        Micro_Fan        = T;
        Selenoid_Valve   = O;
        Infuse_Pump      = B;
    end else begin
        Micro_Fan        = 0;
        Selenoid_Valve   = 0;
        Infuse_Pump      = 0;
    end
end

// Optional: Tambahkan output untuk memudahkan visualisasi status di GTKWave
// Anda dapat melihat reg [1:0] current_state secara langsung.

endmodule