package io.meshtech.koasta.view;

import android.content.Context;
import android.content.res.ColorStateList;
import android.graphics.Bitmap;
import android.util.AttributeSet;
import android.util.TypedValue;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.TextView;

import androidx.annotation.Nullable;
import androidx.constraintlayout.widget.ConstraintLayout;

import io.meshtech.koasta.R;

public class VenueVoteItemView extends ConstraintLayout {
    private TextView title;
    private ViewGroup voteButton;
    private TextView voteButtonTitle;
    private TextView voteDescription;
    private ViewGroup thumb;
    private ImageView imagePlaceholderA;
    private ImageView imagePlaceholderB;
    private ImageView imagePlaceholderC;
    private ImageView imagePlaceholderD;

    public VenueVoteItemView(Context context) {
        super(context);
        init();
    }

    public VenueVoteItemView(Context context, @Nullable AttributeSet attrs) {
        super(context, attrs);
        init();
    }

    public VenueVoteItemView(Context context, @Nullable AttributeSet attrs, int defStyleAttr) {
        super(context, attrs, defStyleAttr);
        init();
    }

    private void init() {
        inflate(getContext(), R.layout.layout_venue_vote_item, this);

        title = findViewById(R.id.venue_item_title);
        voteButton = findViewById(R.id.vote_button);
        voteButtonTitle = findViewById(R.id.vote_button_title);
        voteDescription = findViewById(R.id.vote_description);
        thumb = findViewById(R.id.venue_item_thumb);
        imagePlaceholderA = findViewById(R.id.venue_item_placeholder_a);
        imagePlaceholderB = findViewById(R.id.venue_item_placeholder_b);
        imagePlaceholderC = findViewById(R.id.venue_item_placeholder_c);
        imagePlaceholderD = findViewById(R.id.venue_item_placeholder_d);
        int pixels = (int) TypedValue.applyDimension(TypedValue.COMPLEX_UNIT_DIP, 300, getResources().getDisplayMetrics());
        setMaxWidth(pixels);
    }

    public void setTitle(String title) {
        this.title.setText(title);
    }

    public void setVoteButtonAlpha(float alpha) {
        this.voteButton.setAlpha(alpha);
    }

    public void setVoteButtonTitle(String voteButtonTitle) {
        this.voteButtonTitle.setText(voteButtonTitle);
    }

    public void setVoteDescription(String voteDescription) {
        this.voteDescription.setText(voteDescription);
    }

    public void setThumbTint(ColorStateList tint) {
        thumb.setBackgroundTintList(tint);
    }

    public void setImagePlaceholderA(Bitmap bm) {
        imagePlaceholderA.setImageBitmap(bm);
    }

    public void setImagePlaceholderB(Bitmap bm) {
        imagePlaceholderB.setImageBitmap(bm);
    }

    public void setImagePlaceholderC(Bitmap bm) {
        imagePlaceholderC.setImageBitmap(bm);
    }

    public void setImagePlaceholderD(Bitmap bm) {
        imagePlaceholderD.setImageBitmap(bm);
    }
}
