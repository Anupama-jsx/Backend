using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddPassageCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO [Passages] ([Id], [Content], [CreatedAtUtc]) VALUES
                ('9d000000-0000-4000-8000-000000000001', N'A calm start creates room for quick hands and clear thinking. Keep your rhythm steady as each sentence unfolds.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000002', N'Great typists balance speed with precision. Every accurate letter builds momentum for the words that follow.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000003', N'Progress often arrives quietly through small choices repeated with patience, focus, and a willingness to improve.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000004', N'The best pace is one you can maintain. Let your fingers move smoothly while your eyes stay ahead of the cursor.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000005', N'Practice turns unfamiliar patterns into reliable habits. Trust the process and let accuracy guide your speed.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000006', N'A thoughtful pause can prevent a rushed error. Breathe, reset, and return to the next word with confidence.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000007', N'Fast typing begins with relaxed hands, level shoulders, and attention that stays anchored to the passage.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000008', N'Each race is a fresh chance to discover a better rhythm. Compete with intent and learn from the result.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000009', N'Consistency is a quiet advantage. A dependable pace can outlast a burst of speed that loses control.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000010', N'Focus on the word in front of you, then the one beyond it. The sentence becomes easier one step at a time.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000011', N'Clear goals make practice meaningful. Choose one detail to improve today and celebrate the progress you notice.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000012', N'Speed grows from repetition, but precision grows from attention. Keep both in view during every race.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000013', N'When the timer begins, settle into a rhythm before you chase the next burst of speed.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000014', N'Good technique saves energy. Light touches and steady breathing leave more attention for difficult words.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000015', N'Confidence is built through preparation. The more you practice with care, the easier fast typing feels.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000016', N'A strong finish starts long before the final word. Build accuracy early and carry it through the passage.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000017', N'Notice the punctuation, respect the spacing, and let every completed line reinforce your concentration.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000018', N'Quick fingers work best with a patient mind. Stay composed when a tricky phrase appears on screen.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000019', N'Every mistake offers information. Correct it calmly, recover your pace, and keep moving toward the finish.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000020', N'Competition can reveal your best habits. Use the pressure to sharpen focus instead of rushing ahead.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000021', N'A reliable rhythm turns long passages into manageable pieces. Keep your gaze forward and your hands relaxed.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000022', N'Accuracy gives speed a foundation. Build the foundation first, then let your pace rise naturally.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000023', N'Every focused session makes the keyboard feel more familiar. Familiarity leaves space for faster decisions.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000024', N'The most useful progress is progress you can repeat. Find a tempo that feels controlled and sustainable.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000025', N'Keep the cursor moving with purpose. A measured recovery after an error is better than a frantic rush.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000026', N'Practice transforms attention into instinct. Over time, your hands learn the route through every common pattern.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000027', N'A race rewards awareness. Listen to your rhythm, watch the passage, and make each keypress count.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000028', N'Patience and pace can work together. Slow down just enough to stay accurate, then build momentum again.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000029', N'Good form is invisible when it works well. Your posture, breathing, and focus quietly support every word.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000030', N'Improvement is not a single leap. It is the sum of many careful starts, corrections, and strong finishes.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000031', N'Let the clock add energy, not stress. A calm typist can make better decisions under pressure.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000032', N'Every paragraph has a pulse. Find it early and use that rhythm to guide your hands through the line.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000033', N'Careful reading and quick execution belong together. Look ahead, trust your practice, and stay present.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000034', N'Recovery matters as much as speed. A brief correction made with control protects the rest of your run.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000035', N'A familiar keyboard becomes an extension of your intent when repetition meets deliberate attention.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000036', N'Choose accuracy on the hard words and confidence on the easy ones. Both choices help your score.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000037', N'Focused practice makes fast work feel simpler. Break the passage into phrases and handle one phrase at a time.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000038', N'Your next personal best starts with this sentence. Stay relaxed, stay alert, and enjoy the challenge.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000039', N'Precision is not the enemy of speed. It is the discipline that keeps speed useful when the race gets close.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000040', N'A smooth rhythm is stronger than a hurried burst. Keep your pace even from the first character to the last.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000041', N'Racing well means adapting quickly. Notice the next challenge, adjust your pace, and keep your momentum.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000042', N'The smallest gains can become large results. Return tomorrow with the same curiosity and a little more skill.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000043', N'Stay light on the keys and clear in your thinking. The right balance helps every line feel more natural.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000044', N'A difficult word is only one moment in a longer race. Move through it, reset, and continue with purpose.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000045', N'Every finished passage is evidence that attention can become momentum. Keep building on what you learn.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000046', N'Good competitors respect the clock and their own process. Let each accurate phrase move you closer to the win.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000047', N'Your hands know more than you think. Give them a steady rhythm and let practice do its quiet work.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000048', N'Fast progress comes from deliberate repetition. Repeat the right habits until they become dependable.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000049', N'Keep the goal visible but the next word closer. Finishing well is the result of many small decisions.', SYSUTCDATETIME()),
                ('9d000000-0000-4000-8000-000000000050', N'Type with purpose, learn with humility, and return for the next race ready to improve your best score.', SYSUTCDATETIME());
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM [Passages]
                WHERE [Id] >= '9d000000-0000-4000-8000-000000000001'
                  AND [Id] <= '9d000000-0000-4000-8000-000000000050';
                """);
        }
    }
}
